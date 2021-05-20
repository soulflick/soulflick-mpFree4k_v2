using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using CSCore;
using CSCore.DSP;
using Mpfree4k.Enums;

namespace Equalizer.Visualization
{
    public class SpectrumBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        [DebuggerDisplay("{Value}")]
        protected struct SpectrumPointData
        {
            public int SpectrumPointIndex;
            public double Value;
        }

        private const int ScaleFactorLinear = 9;
        protected const int ScaleFactorSqr = 2;
        protected const double MinDbValue = -90;
        protected const double MaxDbValue = 0;
        protected const double DbScale = (MaxDbValue - MinDbValue);

        private int _fftSize;
        private bool _isXLogScale;
        private int _maxFftIndex;
        private int _maximumFrequency = 20000;
        private int _maximumFrequencyIndex;
        private int _minimumFrequency = 20; //Default spectrum from 20Hz to 20kHz
        private int _minimumFrequencyIndex;
        private ScalingStrategy _scalingStrategy;
        private int[] _spectrumIndexMax;
        private int[] _spectrumLogScaleIndexMax;
        private ISpectrumProvider _spectrumProvider;

        protected int SpectrumResolution;
        private bool _useAverage;

        public int MaximumFrequency
        {
            get => _maximumFrequency;
            set
            {
                if (value <= MinimumFrequency)
                {
                    throw new ArgumentOutOfRangeException("value",
                        "Value must not be less or equal the MinimumFrequency.");
                }
                _maximumFrequency = value;
                UpdateFrequencyMapping();

                Raise(nameof(MaximumFrequency));
            }
        }

        public int MinimumFrequency
        {
            get => _minimumFrequency;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");
                _minimumFrequency = value;
                UpdateFrequencyMapping();

                Raise(nameof(MinimumFrequency));
            }
        }

        [BrowsableAttribute(false)]
        public ISpectrumProvider SpectrumProvider
        {
            get => _spectrumProvider;
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _spectrumProvider = value;

                Raise(nameof(SpectrumProvider));
            }
        }

        public bool IsXLogScale
        {
            get => _isXLogScale;
            set
            {
                _isXLogScale = value;
                UpdateFrequencyMapping();
                Raise(nameof(IsXLogScale));
            }
        }

        public ScalingStrategy ScalingStrategy
        {
            get => _scalingStrategy;
            set
            {
                _scalingStrategy = value;
                Raise(nameof(ScalingStrategy));
            }
        }

        public bool UseAverage
        {
            get => _useAverage;
            set
            {
                _useAverage = value;
                Raise(nameof(UseAverage));
            }
        }

        [BrowsableAttribute(false)]
        public FftSize FftSize
        {
            get => (FftSize)_fftSize;

            protected set
            {
                if ((int)Math.Log((int)value, 2) % 1 != 0)
                    throw new ArgumentOutOfRangeException("value");

                _fftSize = (int)value;
                _maxFftIndex = _fftSize / 2 - 1;

                Raise(nameof(FftSize));
            }
        }

        protected virtual void UpdateFrequencyMapping()
        {
            _maximumFrequencyIndex = Math.Min(_spectrumProvider.GetFftBandIndex(MaximumFrequency) + 1, _maxFftIndex);
            _minimumFrequencyIndex = Math.Min(_spectrumProvider.GetFftBandIndex(MinimumFrequency), _maxFftIndex);

            int actualResolution = SpectrumResolution;

            int indexCount = _maximumFrequencyIndex - _minimumFrequencyIndex;
            double linearIndexBucketSize = Math.Round(indexCount / (double)actualResolution, 3);

            _spectrumIndexMax = _spectrumIndexMax.CheckBuffer(actualResolution, true);
            _spectrumLogScaleIndexMax = _spectrumLogScaleIndexMax.CheckBuffer(actualResolution, true);

            double maxLog = Math.Log(actualResolution, actualResolution);
            for (int i = 1; i < actualResolution; i++)
            {
                int logIndex =
                    (int)((maxLog - Math.Log((actualResolution + 1) - i, (actualResolution + 1))) * indexCount) +
                    _minimumFrequencyIndex;

                _spectrumIndexMax[i - 1] = _minimumFrequencyIndex + (int)(i * linearIndexBucketSize);
                _spectrumLogScaleIndexMax[i - 1] = logIndex;
            }

            if (actualResolution > 0)
            {
                _spectrumIndexMax[_spectrumIndexMax.Length - 1] =
                    _spectrumLogScaleIndexMax[_spectrumLogScaleIndexMax.Length - 1] = _maximumFrequencyIndex;
            }
        }

        protected virtual SpectrumPointData[] CalculateSpectrumPoints(double maxValue, float[] fftBuffer)
        {
            var dataPoints = new List<SpectrumPointData>();

            double value0 = 0, value = 0;
            double lastValue = 0;
            double actualMaxValue = maxValue;
            int spectrumPointIndex = 0;

            for (int i = _minimumFrequencyIndex; i <= _maximumFrequencyIndex; i++)
            {
                switch (ScalingStrategy)
                {
                    case ScalingStrategy.Decibel:
                        value0 = (((20 * Math.Log10(fftBuffer[i])) - MinDbValue) / DbScale) * actualMaxValue;
                        break;
                    case ScalingStrategy.Linear:
                        value0 = (fftBuffer[i] * ScaleFactorLinear) * actualMaxValue;
                        break;
                    case ScalingStrategy.Sqrt:
                        value0 = ((Math.Sqrt(fftBuffer[i])) * ScaleFactorSqr) * actualMaxValue;
                        break;
                }

                bool recalc = true;

                value = Math.Max(0, Math.Max(value0, value));

                while (spectrumPointIndex <= _spectrumIndexMax.Length - 1 &&
                       i ==
                       (IsXLogScale
                           ? _spectrumLogScaleIndexMax[spectrumPointIndex]
                           : _spectrumIndexMax[spectrumPointIndex]))
                {
                    if (!recalc)
                        value = lastValue;

                    if (value > maxValue)
                        value = maxValue;

                    if (_useAverage && spectrumPointIndex > 0)
                        value = (lastValue + value) / 2.0;

                    dataPoints.Add(new SpectrumPointData { SpectrumPointIndex = spectrumPointIndex, Value = value });

                    lastValue = value;
                    value = 0.0;
                    spectrumPointIndex++;
                    recalc = false;
                }
            }

            return dataPoints.ToArray();
        }
    }
}
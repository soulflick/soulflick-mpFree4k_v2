﻿using System;
using System.Collections.Generic;
using CSCore.DSP;

namespace Equalizer.Visualization
{
    public interface ISpectrumProvider
    {
        bool GetFftData(float[] fftBuffer, object context);
        int GetFftBandIndex(float frequency);
    }

    public class SpectrumProvider : FftProvider, ISpectrumProvider
    {
        private readonly int _sampleRate;
        private readonly List<object> _contexts = new List<object>();

        public SpectrumProvider(int channels, int sampleRate, FftSize fftSize)
            : base(channels, fftSize)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException("sampleRate");
            _sampleRate = sampleRate;
        }

        public int GetFftBandIndex(float frequency)
        {
            int fftSize = (int)FftSize;
            double f = _sampleRate / 2.0;
            return (int)((frequency / f) * (fftSize / 2));
        }

        public bool GetFftData(float[] fftResultBuffer, object context)
        {
            if (_contexts.Contains(context))
                return false;

            _contexts.Add(context);
            GetFftData(fftResultBuffer);
            return true;
        }

        public override void Add(float[] samples, int count)
        {
            base.Add(samples, count);
            if (count > 0)
                _contexts.Clear();
        }

        public override void Add(float left, float right)
        {
            base.Add(left, right);
            _contexts.Clear();
        }
    }
}
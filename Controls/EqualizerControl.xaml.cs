using CSCore.Streams.Effects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WPFEqualizer;

namespace MpFree4k.Controls
{
    public class ValueObject :  INotifyPropertyChanged
    {

        private double value = 0;
        public double Value { get => value; 
            set
            {
                this.value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                ValueChanged?.Invoke(this, null);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public static event PropertyChangedEventHandler ValueChanged;

    }

    public partial class EqualizerControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        List<double> originalGain = new List<double>(10);
        public int BandCount { get; set; } = 10;

        public void RaiseProperty(string propertyName) => 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public EqualizerControl()
        {
            InitializeComponent();
            this.DataContext = this;

            if (SpectrumViewModel.Instance?._equalizer == null)
                return;

            tbNotify.Visibility = Visibility.Collapsed;

            equalizer = SpectrumViewModel.Instance._equalizer;
            BandCount = equalizer.SampleFilters.Count;
            originalGain = SpectrumViewModel.Instance._equalizer.SampleFilters.ToList().Select(f => f.AverageGainDB).ToList();

            for (int i = 0; i < BandCount; i++)
            {
                double eqVal = originalGain[i];
                EqBands.Add(new ValueObject() { Value = eqVal });
            }

            ValueObject.ValueChanged += ValueObject_ValueChanged;

            RaiseProperty(nameof(EqBands));
        }

        private void ValueObject_ValueChanged(object sender, PropertyChangedEventArgs e)
        {
            Apply();
        }

        private Equalizer equalizer;
        private List<ValueObject> _eqBands = new List<ValueObject>();
        public List<ValueObject> EqBands
        {
            get => _eqBands;
            set
            {
                _eqBands = value;
                RaiseProperty(nameof(EqBands));
            }
        }

        public  void Set(double[] values)
        {
            if (values.Length != SpectrumViewModel.EqualizerBandCount || values.Length != EqBands.Count)
                throw new ArgumentException("Euqalizer band resolution mistake.");

            for (int i = 0; i < EqBands.Count; i++)
                EqBands[i].Value = values[i];
        }

        public void Reset()
        {
            for (int e = 0; e < EqBands.Count; e++)
                EqBands[e].Value = 0; // originalGain[e];
        }

        public void Apply()
        {
            for (int  i = 0; i < BandCount; i++)
            {
                double val = Math.Max(-20, EqBands[i].Value);
                val = Math.Min(val, 20);
                equalizer.SampleFilters[i].AverageGainDB = val;
            }
        }
    }
}

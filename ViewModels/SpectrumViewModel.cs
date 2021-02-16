using CSCore;
using CSCore.Codecs;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Timers;
using WPFEqualizer.Utils;
using WPFEqualizer.Visualization;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;
using WPFEqualizer.Controls;

namespace WPFEqualizer
{
    public enum GraphType
    {
        Bar,
        Line,
        Band
    }

    public class SpectrumViewModel : INotifyPropertyChanged

    {
        public event PropertyChangedEventHandler PropertyChanged;
        public LinearSpectrum _control;
        public LineSpectrum _lineSpectrum;
        public IWaveSource _source;
       
        public WasapiCapture _capture;
        public CSCore.Streams.Effects.Equalizer _equalizer;
        public SimpleNotificationSource _simpleNotificationSource;
        public SingleBlockNotificationStream _singleBlockNotificationStream;
        public BasicSpectrumProvider _spectrumProvider;

        private Timer _waveTimer;
        private Point? _tipPos;
        private int[] _tipPositions;

        public Color Background = Color.FromArgb(255, 30, 30, 30);
        public Color BarStartBrush = Color.SteelBlue;
        public Color BarEndBrush = Color.Yellow;
        public Color LineBrush = Color.LightGray;
        public Color FillBrush = Color.Gray;

        public double Spacing = 1;
        public static SpectrumViewModel Instance;
        public ISoundOut _soundOut;

        private GraphType _graphType = GraphType.Bar;
        public GraphType GraphType
        {
            get => _graphType;
            set
            {
                _graphType = value;
                CreateSpectrum();
            }
        }

        private int _barGap = 1;
        public int BarGap
        {
            get => _barGap;
            set
            {
                _barGap = value;
                if (_lineSpectrum != null)
                    _lineSpectrum.BarGap = value;
            }
        }

        private int _barCount = 120;
        public int BarCount
        {
            get => _barCount;
            set
            {
                _barCount = value;
                if (_lineSpectrum != null)
                    _lineSpectrum.BarCount = value;
            }
        }

        private int _barSegment = 4;
        public int BarSegment
        {
            get => _barSegment;
            set
            {
                _barSegment = value;
                if (_lineSpectrum != null)
                    _lineSpectrum.BarSegment = value;
            }
        }

        public SpectrumViewModel(LinearSpectrum control)
        {
            this._control = control;
            this._waveTimer = new Timer(10);
            this._waveTimer.Elapsed += WaveTimer_Elapsed;

            _tipPositions = new int[BarCount];
            for (int i = 0; i < BarCount; i++)
                _tipPositions[i] = 0;

            Instance = this;

        }

        public Size ViewPort;

        private string mediaFile = null;
        public string MediaFile
        {
            get { return mediaFile; }
            set
            {
                mediaFile = value;
                RaisePropertyChanged(nameof(MediaFile));
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void WaveTimer_Elapsed(object sender, EventArgs e)
        {
            if (ViewPort.Width == 0)
                return;

            GenerateLineSpectrum();
        }

        public void RemoveSpectrum()
        {
            if (ViewPort.Width == 0 || ViewPort.Height == 0)
            {
                _control = null;
                return;
            }

            var bitmap = new Bitmap(ViewPort.Width, ViewPort.Height);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Background);
            }

            if (_control != null) 
                _control.ImageSpectrum.Source = BMP.BitmapToImageSource(bitmap);

            _control = null;
        }

        public void GenerateLineSpectrum()
        {
            if (_control == null)
                return;

            if (_lineSpectrum == null)
                CreateSpectrum();

            Bitmap image = _lineSpectrum?.CreateSpectrumLine(ViewPort, BarStartBrush, BarEndBrush, Background, LineBrush, FillBrush, true);

            if (image == null)
                return;

            _control?.Dispatcher.Invoke(() =>
                { if (_control != null) _control.ImageSpectrum.Source = BMP.BitmapToImageSource(image); });

            if (image != null)
                image.Dispose();
        }

        public IWaveSource source = null;
        public ISampleSource sampleSource = null;
        public void Init()
        {
            if (source != null)
                source.Dispose();

            const FftSize fftSize = FftSize.Fft4096;
            _capture = new WasapiLoopbackCapture();

            source = CodecFactory.Instance.GetCodec(MediaFile);
            _spectrumProvider = new BasicSpectrumProvider(source.WaveFormat.Channels, source.WaveFormat.SampleRate, fftSize);

            CreateSpectrum();

            Equalizer newEqualizer = null;

            source = source
               .ChangeSampleRate(44100)
               .AppendSource(x => CSCore.Streams.Effects.Equalizer.Create10BandEqualizer(x.ToSampleSource()), out newEqualizer)
               .AppendSource(x => new SingleBlockNotificationStream(x), out _singleBlockNotificationStream)
               .AppendSource(x => new SimpleNotificationSource(x) { Interval = 100 }, out _simpleNotificationSource)
               .ToWaveSource();

            SetEqualizer(0);

            SetEqualizer(newEqualizer);

            sampleSource = source.ToSampleSource();

            //_simpleNotificationSource.BlockRead += notifysource_BlockRead;
            _singleBlockNotificationStream.SingleBlockRead += (s, a) => _spectrumProvider.Add(a.Left, a.Right);

            if (WasapiOut.IsSupportedOnCurrentPlatform)
                _soundOut = new WasapiOut();
            else
                _soundOut = new DirectSoundOut();

            _soundOut.Initialize(source);

            _waveTimer.Start();

        }

        private void SetEqualizer(Equalizer newEQ)
        {
            if (newEQ == null)
                return;

            if (_equalizer ==  null || _equalizer.SampleFilters.Count !=  newEQ.SampleFilters.Count)
            {
                _equalizer = newEQ;
                return;
            }


            for (int i = 0; i <_equalizer.SampleFilters.Count; i++)
                newEQ.SampleFilters[i].AverageGainDB = _equalizer.SampleFilters[i].AverageGainDB;

            _equalizer = newEQ;
        }

        public void CreateSpectrum()
        {
            if (_spectrumProvider == null || _control == null)
                return;

            const FftSize fftSize = FftSize.Fft4096;

            _lineSpectrum = new LineSpectrum(fftSize, BarCount)
            {
                SpectrumProvider = _spectrumProvider,
                UseAverage = true,
                BarCount = this.BarCount,
                BarSpacing = this.Spacing,
                IsXLogScale = true,
                ScalingStrategy = ScalingStrategy.Sqrt,
                BarGap = BarGap,
                BarSegment = BarSegment,
                GraphType = GraphType
            };
        }

        private void SetEqualizer(double gain)
        {
            if (_equalizer == null)
                return;

            gain = Math.Min(20, gain);
            for (int f = 0; f < _equalizer.SampleFilters.Count; f++)
            {
                EqualizerFilter filter = _equalizer.SampleFilters[f];
                filter.AverageGainDB = gain;

                for (int j = 0; j < filter.Filters.Count; j++)
                    filter.Filters[j].GainDB = gain;
            }
        }


        public void ApplyTipPos(Point pos)
        {
            return;

            Point p = (Point)pos;
            int height = ViewPort.Height;

            int filterIndex = (int)(((double)BarCount / ViewPort.Width) * p.X);
            float y = (float)(Math.Floor((double)p.Y / (double)5) * 5);
            y = ((float)100 / ViewPort.Height) * (ViewPort.Height - y);
            _tipPositions[filterIndex] = (int)y;

            int MaxDB = 20;
            float percent = (float)1 / ViewPort.Height * (ViewPort.Height - (float)p.Y);
            var value = Math.Min(20, percent * MaxDB);

            if (filterIndex < 10)
            {
                EqualizerFilter filter = _equalizer.SampleFilters[filterIndex];
                filter.AverageGainDB = value;

                for (int j = 0; j < filter.Filters.Count; j++)
                    filter.Filters[j].GainDB = value;

            }

            GenerateLineSpectrum();
        }

        public void SetTipPos(System.Windows.Point? tipPos)
        {
            return;

            if (tipPos == null)
                _lineSpectrum.MousePoint = null;
            else
                _lineSpectrum.MousePoint = Utils.Drawing.ToPoint((Point)tipPos);

            GenerateLineSpectrum();
        }
    }
}

using Mpfree4k.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Equalizer.Controls
{
    public partial class LinearSpectrum : UserControl
    {
        private SpectrumViewModel viewModel;

        public static readonly DependencyProperty BarGapProperty = DependencyProperty.Register(
          nameof(BarGap), typeof(int), typeof(LinearSpectrum), new PropertyMetadata(1));

        public static readonly DependencyProperty BarSegmentProperty = DependencyProperty.Register(
          nameof(BarSegment), typeof(int), typeof(LinearSpectrum), new PropertyMetadata(4));

        public static readonly DependencyProperty ControlBackgroundProperty = DependencyProperty.Register(
          nameof(ControlBackground), typeof(System.Windows.Media.SolidColorBrush), typeof(LinearSpectrum), new PropertyMetadata(System.Windows.Media.Brushes.DarkGray));

        public static readonly DependencyProperty BarStartBrushProperty = DependencyProperty.Register(
          nameof(BarStartBrush), typeof(System.Windows.Media.SolidColorBrush), typeof(LinearSpectrum), new PropertyMetadata(System.Windows.Media.Brushes.SteelBlue));

        public static readonly DependencyProperty BarEndBrushProperty = DependencyProperty.Register(
          nameof(BarEndBrush), typeof(System.Windows.Media.SolidColorBrush), typeof(LinearSpectrum), new PropertyMetadata(System.Windows.Media.Brushes.Yellow));

        public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(
          nameof(LineBrush), typeof(System.Windows.Media.SolidColorBrush), typeof(LinearSpectrum), new PropertyMetadata(System.Windows.Media.Brushes.Gainsboro));

        public static readonly DependencyProperty FillBrushProperty = DependencyProperty.Register(
          nameof(FillBrush), typeof(System.Windows.Media.SolidColorBrush), typeof(LinearSpectrum), new PropertyMetadata(System.Windows.Media.Brushes.Gray));

        public static readonly DependencyProperty BarCountProperty = DependencyProperty.Register(
          nameof(BarCount), typeof(int), typeof(LinearSpectrum), new PropertyMetadata(48));

        public static readonly DependencyProperty GraphTypeProperty = DependencyProperty.Register(
          nameof(GraphType), typeof(GraphType), typeof(LinearSpectrum), new PropertyMetadata(GraphType.Bar));

        
        public int BarGap
        {
            get => (int)GetValue(BarGapProperty);
            set
            {
                SetValue(BarGapProperty, value);
                viewModel.BarGap = value;
            }
        }

        public int BarSegment
        {
            get => (int)GetValue(BarSegmentProperty);
            set
            {
                SetValue(BarSegmentProperty, value);
                viewModel.BarSegment = value;
            }
        }

        public System.Windows.Media.SolidColorBrush ControlBackground
        {
            get => (System.Windows.Media.SolidColorBrush)GetValue(ControlBackgroundProperty);
            set
            {
                SetValue(ControlBackgroundProperty, value);
                if (viewModel != null)
                    viewModel.Background = Utils.ColorUtils.ToDrawingColor(value);
            }
        }

        public System.Windows.Media.SolidColorBrush BarStartBrush
        {
            get => (System.Windows.Media.SolidColorBrush)GetValue(BarStartBrushProperty);
            set
            {
                SetValue(BarStartBrushProperty, value);
                if (viewModel != null)
                    viewModel.BarStartBrush = Utils.ColorUtils.ToDrawingColor(value);
            }
        }

        public System.Windows.Media.SolidColorBrush BarEndBrush
        {
            get => (System.Windows.Media.SolidColorBrush)GetValue(BarEndBrushProperty);
            set
            {
                SetValue(BarEndBrushProperty, value);
                if (viewModel != null)
                    viewModel.BarEndBrush = Utils.ColorUtils.ToDrawingColor(value);
            }
        }

        public System.Windows.Media.SolidColorBrush LineBrush
        {
            get => (System.Windows.Media.SolidColorBrush)GetValue(LineBrushProperty);
            set
            {
                SetValue(LineBrushProperty, value);
                if (viewModel != null)
                    viewModel.LineBrush = Utils.ColorUtils.ToDrawingColor(value);
            }
        }

        public System.Windows.Media.SolidColorBrush FillBrush
        {
            get => (System.Windows.Media.SolidColorBrush)GetValue(FillBrushProperty);
            set
            {
                SetValue(FillBrushProperty, value);
                if (viewModel != null)
                    viewModel.FillBrush = Utils.ColorUtils.ToDrawingColor(value);
            }
        }

        public int BarCount
        {
            get => (int)GetValue(BarCountProperty);
            set
            {
                SetValue(BarCountProperty, value);
                viewModel.BarCount = value;
            }
        }

        public GraphType GraphType
        {
            get => (GraphType)GetValue(GraphTypeProperty);
            set
            {
                SetValue(GraphTypeProperty, value);
                if (viewModel != null) viewModel.GraphType = value;
            }
        }

        public LinearSpectrum()
        {
            InitializeComponent();
            Loaded += Spectrum_Loaded;
            SizeChanged += Spectrum_SizeChanged;
        }

        public void SetViewModel(SpectrumViewModel vm) => DataContext = viewModel = vm;

        public void Redraw() => viewModel.GenerateLineSpectrum();

        private void Spectrum_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ImageSpectrum.Width = GridImage.ActualWidth;
            ImageSpectrum.Height = GridImage.ActualHeight;
            SetViewPort();
        }

        private void Spectrum_Loaded(object sender, RoutedEventArgs e)
        {
            SetViewPort();
            ApplyProperties();
        }

        public void ApplyProperties()
        {
            if (viewModel == null)
                return;

            viewModel.BarGap = (int)this.GetValue(BarGapProperty);
            viewModel.BarSegment = (int)this.GetValue(BarSegmentProperty);
            viewModel.Background = Utils.ColorUtils.ToDrawingColor((System.Windows.Media.Brush)this.GetValue(ControlBackgroundProperty));
            viewModel.BarCount = (int)this.GetValue(BarCountProperty);
            viewModel.GraphType = (GraphType)this.GetValue(GraphTypeProperty);
            viewModel.BarStartBrush = Utils.ColorUtils.ToDrawingColor((System.Windows.Media.Brush)this.GetValue(BarStartBrushProperty));
            viewModel.BarEndBrush = Utils.ColorUtils.ToDrawingColor((System.Windows.Media.Brush)this.GetValue(BarEndBrushProperty));
            viewModel.LineBrush = Utils.ColorUtils.ToDrawingColor((System.Windows.Media.Brush)this.GetValue(LineBrushProperty));
            viewModel.FillBrush = Utils.ColorUtils.ToDrawingColor((System.Windows.Media.Brush)this.GetValue(FillBrushProperty));
        }

        public void SetViewPort()
        {
            if (viewModel == null)
                return;

            viewModel.ViewPort = new System.Drawing.Size((int)ImageSpectrum.Width, (int)ImageSpectrum.Height);
        }

        private void ImageSpectrum_MouseLeave(object sender, MouseEventArgs e)
        {
            if (viewModel == null)
                return;

            viewModel.SetTipPos(null);
        }

        private void ImageSpectrum_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (viewModel == null)
                return;

            Point pos = Mouse.GetPosition(ImageSpectrum);
            viewModel.ApplyTipPos(pos);
        }
    }

}

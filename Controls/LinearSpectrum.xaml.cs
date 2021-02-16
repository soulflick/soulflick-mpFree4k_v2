using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFEqualizer.Controls
{
    public partial class LinearSpectrum : UserControl
    {
        public static readonly DependencyProperty BarGapProperty = DependencyProperty.Register(
          "BarGap", typeof(int), typeof(LinearSpectrum), new PropertyMetadata(1));

        public static readonly DependencyProperty BarSegmentProperty = DependencyProperty.Register(
          "BarSegment", typeof(int), typeof(LinearSpectrum), new PropertyMetadata(4));

        public static readonly DependencyProperty ControlBackgroundProperty = DependencyProperty.Register(
          "ControlBackground", typeof(System.Windows.Media.SolidColorBrush), typeof(LinearSpectrum), new PropertyMetadata(System.Windows.Media.Brushes.DarkGray));

        public static readonly DependencyProperty BarStartBrushProperty = DependencyProperty.Register(
          "BarStartBrush", typeof(System.Windows.Media.SolidColorBrush), typeof(LinearSpectrum), new PropertyMetadata(System.Windows.Media.Brushes.SteelBlue));

        public static readonly DependencyProperty BarEndBrushProperty = DependencyProperty.Register(
          "BarEndBrush", typeof(System.Windows.Media.SolidColorBrush), typeof(LinearSpectrum), new PropertyMetadata(System.Windows.Media.Brushes.Yellow));

        public static readonly DependencyProperty LineBrushProperty = DependencyProperty.Register(
          "LineBrush", typeof(System.Windows.Media.SolidColorBrush), typeof(LinearSpectrum), new PropertyMetadata(System.Windows.Media.Brushes.Gainsboro));

        public static readonly DependencyProperty FillBrushProperty = DependencyProperty.Register(
          "FillBrush", typeof(System.Windows.Media.SolidColorBrush), typeof(LinearSpectrum), new PropertyMetadata(System.Windows.Media.Brushes.Gray));

        public static readonly DependencyProperty BarCountProperty = DependencyProperty.Register(
          "BarCount", typeof(int), typeof(LinearSpectrum), new PropertyMetadata(48));

        public static readonly DependencyProperty GraphTypeProperty = DependencyProperty.Register(
          "GraphType", typeof(GraphType), typeof(LinearSpectrum), new PropertyMetadata(GraphType.Bar));

        

        public int BarGap
        {
            get { return (int)this.GetValue(BarGapProperty); }
            set
            {
                this.SetValue(BarGapProperty, value);
                viewModel.BarGap = value;
            }
        }

        public int BarSegment
        {
            get { return (int)this.GetValue(BarSegmentProperty); }
            set
            {
                this.SetValue(BarSegmentProperty, value);
                viewModel.BarSegment = value;
            }
        }

        public System.Windows.Media.SolidColorBrush ControlBackground
        {
            get { return (System.Windows.Media.SolidColorBrush)this.GetValue(ControlBackgroundProperty); }
            set
            {
                this.SetValue(ControlBackgroundProperty, value);
                if (viewModel != null)
                    viewModel.Background = Utils.ColorUtils.ToDrawingColor(value);
            }
        }

        public System.Windows.Media.SolidColorBrush BarStartBrush
        {
            get { return (System.Windows.Media.SolidColorBrush)this.GetValue(BarStartBrushProperty); }
            set
            {
                this.SetValue(BarStartBrushProperty, value);
                if (viewModel != null)
                    viewModel.BarStartBrush = Utils.ColorUtils.ToDrawingColor(value);
            }
        }

        public System.Windows.Media.SolidColorBrush BarEndBrush
        {
            get { return (System.Windows.Media.SolidColorBrush)this.GetValue(BarEndBrushProperty); }
            set
            {
                this.SetValue(BarEndBrushProperty, value);
                if (viewModel != null)
                    viewModel.BarEndBrush = Utils.ColorUtils.ToDrawingColor(value);
            }
        }

        public System.Windows.Media.SolidColorBrush LineBrush
        {
            get { return (System.Windows.Media.SolidColorBrush)this.GetValue(LineBrushProperty); }
            set
            {
                this.SetValue(LineBrushProperty, value);
                if (viewModel != null)
                    viewModel.LineBrush = Utils.ColorUtils.ToDrawingColor(value);
            }
        }

        public System.Windows.Media.SolidColorBrush FillBrush
        {
            get { return (System.Windows.Media.SolidColorBrush)this.GetValue(FillBrushProperty); }
            set
            {
                this.SetValue(FillBrushProperty, value);
                if (viewModel != null)
                    viewModel.FillBrush = Utils.ColorUtils.ToDrawingColor(value);
            }
        }

        public int BarCount
        {
            get { return (int)this.GetValue(BarCountProperty); }
            set
            {
                this.SetValue(BarCountProperty, value);
                viewModel.BarCount = value;
            }
        }

        public GraphType GraphType
        {
            get { return (GraphType)this.GetValue(GraphTypeProperty); }
            set
            {
                this.SetValue(GraphTypeProperty, value);
                viewModel.GraphType = value;
            }
        }

        private SpectrumViewModel viewModel;

        public LinearSpectrum()
        {
            InitializeComponent();
            Loaded += Spectrum_Loaded;
            SizeChanged += Spectrum_SizeChanged;
        }

        private void Spectrum_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ImageSpectrum.Width = GridImage.ActualWidth;
            ImageSpectrum.Height = GridImage.ActualHeight;
            SetViewPort();
        }

        public void SetViewModel(SpectrumViewModel vm)
        {
            this.DataContext = viewModel = vm;
        }

        private void Spectrum_Loaded(object sender, RoutedEventArgs e)
        {
            SetViewPort();
            ApplyProperties();
        }

        public void Redraw()
        {
            viewModel.GenerateLineSpectrum();
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

            System.Windows.Point pos = Mouse.GetPosition(ImageSpectrum);
            viewModel.ApplyTipPos(pos);
        }
    }

}

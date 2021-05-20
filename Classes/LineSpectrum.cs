using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using CSCore.DSP;
using Mpfree4k.Enums;
using Equalizer.Utils;

namespace Equalizer.Visualization
{
    public struct MaximumTip
    {
        public int height;
        public DateTime time;
        public int h_;
        public float t_;
    }

    public class LineSpectrum : SpectrumBase
    {
        private int _barCount;
        private double _barSpacing;
        private double _barWidth;
        private Size _currentSize;
        private MaximumTip[] _tips;

        public int numLines = 18;
        public int BarGap = 1;
        public int BarSegment = 4;
        public Point? MousePoint;
        public GraphType GraphType = GraphType.Line;

        public LineSpectrum(FftSize fftSize, int numBars) : base()
        {
            FftSize = fftSize;
            BarCount = numBars;
            ScalingStrategy = ScalingStrategy.Linear;
        }

        [Browsable(false)]
        public double BarWidth => _barWidth;

        public double BarSpacing
        {
            get => _barSpacing;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value");

                _barSpacing = value;

                UpdateFrequencyMapping();
                Raise(nameof(BarSpacing));
                Raise(nameof(BarWidth));
            }
        }

        public int BarCount
        {
            get { return _barCount; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value");

                _barCount = value;

                ResetTips();

                SpectrumResolution = value;

                UpdateFrequencyMapping();
                Raise(nameof(BarCount));
                Raise(nameof(BarWidth));
            }
        }

        [BrowsableAttribute(false)]
        public Size CurrentSize
        {
            get { return _currentSize; }
            protected set
            {
                _currentSize = value;
                Raise(nameof(CurrentSize));
            }
        }

        private void ResetTips()
        {
            _tips = new MaximumTip[_barCount];
            for (int t = 0; t < _tips.Length; t++)
            {
                _tips[t].time = DateTime.Now;
            }
        }

        public Bitmap DrawSpectrumLine(Size size, Brush gradientBrush, Color background, Color lineColor, Color fillColor, bool highQuality)
        {
            if (!size.IsRealSize())
                return null;

            UpdateFrequencyMappingIfNessesary(size);

            var fftBuffer = new float[(int)FftSize];

            if (SpectrumProvider.GetFftData(fftBuffer, this))
            {

                using (var pen = new Pen(gradientBrush, (float)_barWidth))
                {
                    var bitmap = new Bitmap(size.Width, size.Height);

                    using (Graphics graphics = Graphics.FromImage(bitmap))
                    {
                        PrepareGraphics(graphics, highQuality);
                        graphics.Clear(background);

                        CreateSpectrumInternal(graphics, pen, fftBuffer, background, lineColor, fillColor, size);
                    }

                    return bitmap;
                }
            }
            return null;
        }

        public Bitmap CreateSpectrumLine(Size size, Color gradientStartColor, Color gradientEndColor, Color background, Color lineColor, Color fillColor, bool highQuality)
        {
            UpdateFrequencyMappingIfNessesary(size);

            using (Brush brush = new LinearGradientBrush(new RectangleF(0, 0, (float)_barWidth, size.Height), gradientEndColor,
                    gradientStartColor, LinearGradientMode.Vertical))
            {
                return DrawSpectrumLine(size, brush, background, lineColor, fillColor, highQuality);
            }
        }

        private void DrawLine(Graphics graphics, Pen pen, PointF p1, PointF p2, int gap, int segment)
        {
            if (BarGap == 0)
            {
                graphics.DrawLine(pen, p1, p2);
                return;
            }

            int maxtop = (int)p2.Y;
            int top = (int)p1.Y;
            PointF p3 = p2;

            while (top > p2.Y)
            {
                top -= segment;
                top = Math.Max(top, maxtop);

                p3.Y = top;

                graphics.DrawLine(pen, p1, p3);
                p1.Y = top - gap;
                top -= gap;
            }
        }

        private void CreateBandSpectrum(Graphics graphics, float[] fftBuffer, Color background, Color lineColor, Color fillColor, Size size)
        {
            int height = size.Height;
            SpectrumPointData[] spectrumPoints = CalculateSpectrumPoints(height, fftBuffer);
            SpectrumLines.Add(spectrumPoints);

            if (SpectrumLines.Count < 2)
                SpectrumLines.Add(spectrumPoints);

            while (SpectrumLines.Count > 2)
                SpectrumLines.RemoveAt(0);


            int numPoints = SpectrumLines[0].Length;
            double[,] yCoords = new double[2, numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                double min = SpectrumLines[0][i].Value;
                double max = SpectrumLines[1][i].Value;
                double c = min;

                if (max < min)
                {
                    min = max;
                    max = c;
                }

                yCoords[0, i] = min;
                yCoords[1, i] = max;
            }

            List<PointF> points = new List<PointF>();
            for (int i = 0; i < numPoints; i++)
            {
                double xCoord = (_barWidth * i) + (BarSpacing * i) + 1 + _barWidth / 2;
                double yCoord = yCoords[1, i];
                yCoord = height - yCoord - 0.3;

                points.Add(new Point((int)xCoord, (int)yCoord));
            }

            for (int i = numPoints-1; i >= 0; i--)
            {
                double xCoord = (_barWidth * i) + (BarSpacing * i) + 1 + _barWidth / 2;
                double yCoord = yCoords[0, i];
                yCoord = height - yCoord +  0.3;

                points.Add(new Point((int)xCoord, (int)yCoord));
            }

            Pen pen = new Pen(lineColor, 1.95f);
            var brush = new SolidBrush(fillColor);

            graphics.DrawPolygon(pen, points.ToArray());
            graphics.FillPolygon(brush, points.ToArray());
        }

        List<SpectrumPointData[]> SpectrumLines = new List<SpectrumPointData[]>();
        private void CreateLineSpectrum(Graphics graphics, float[] fftBuffer, Color background, Color lineColor, Size size)
        {
            int height = size.Height;
            SpectrumPointData[] spectrumPoints = CalculateSpectrumPoints(height, fftBuffer);
            SpectrumLines.Add(spectrumPoints);

            while (SpectrumLines.Count > numLines)
                SpectrumLines.RemoveAt(0);

            int _localNumLines = Math.Min(numLines, SpectrumLines.Count);
            float lineWidth = 0.7f;

            if (_localNumLines <= 2) lineWidth = 2;

            double _deltaR = (lineColor.R - background.R) / _localNumLines;
            double _deltaG = (lineColor.G - background.G) / _localNumLines;
            double _deltaB = (lineColor.B - background.B) / _localNumLines;

            for (int lineNumber = 0; lineNumber < Math.Min(_localNumLines, SpectrumLines.Count); lineNumber++)
            {
                Color penColor = Color.FromArgb(
                    (int)(background.R + _deltaR * (lineNumber + 1)),
                    (int)(background.G + _deltaG * (lineNumber + 1)),
                    (int)(background.B + _deltaB * (lineNumber + 1)));

                var pen = new Pen(penColor, lineWidth);
                PointF p1 = new PointF(0, height - (float)SpectrumLines[lineNumber][0].Value);

                for (int i = 0; i < SpectrumLines[lineNumber].Length; i++)
                {
                    SpectrumPointData p = SpectrumLines[lineNumber][i];
                    int barIndex = p.SpectrumPointIndex;
                    double xCoord = (_barWidth * barIndex) + (BarSpacing * barIndex) + 1 + _barWidth / 2;
                    int barheight = (int)p.Value;
                    float yCoord = (height - (float)p.Value);

                    var p2 = new PointF((float)xCoord, yCoord);

                    graphics.DrawLine(pen, p1, p2);
                    p1 = p2;
                }
            }
        }

        private void CreateSpectrumInternal(Graphics graphics, Pen pen, float[] fftBuffer, Color background, Color lineColor, Color fillColor, Size size)
        {
            if (GraphType == GraphType.Line)
            {
                CreateLineSpectrum(graphics, fftBuffer, background, lineColor, size);
                return;
            }
            else if (GraphType == GraphType.Band)
            {
                CreateBandSpectrum(graphics, fftBuffer, background, lineColor, fillColor, size);
                return;
            }

            int height = size.Height;
            SpectrumPointData[] spectrumPoints = CalculateSpectrumPoints(height, fftBuffer);

            for (int i = 0; i < spectrumPoints.Length; i++)
            {
                SpectrumPointData p = spectrumPoints[i];
                int barIndex = p.SpectrumPointIndex;
                double xCoord = (_barWidth * barIndex) + (BarSpacing * barIndex) + 1 + _barWidth / 2;

                var p1 = new PointF((float)xCoord, height);
                var p2 = new PointF((float)xCoord, height - (float)p.Value - 1);

                int barheight = (int)p.Value;
                if (_tips[i].height < barheight)
                {
                    _tips[i].height = barheight;
                    _tips[i].time = DateTime.Now.AddMilliseconds(500);
                    _tips[i].h_ = barheight;
                    _tips[i].t_ = (500 / (float)height) * barheight;
                }

                DrawLine(graphics, pen, p1, p2, BarGap, BarSegment);
            }

            var tipPpen = new Pen(Brushes.LightGray, (float)_barWidth);

            for (int i = 0; i < spectrumPoints.Length; i++)
            {
                int tipHeight = _tips[i].height;

                int timeDiff = (int)(_tips[i].time - DateTime.Now).TotalMilliseconds;
                if (timeDiff < 0)
                {
                    int elapsed = Math.Abs(timeDiff);
                    tipHeight = (int)Math.Max(0, _tips[i].h_ - (_tips[i].h_ / _tips[i].t_) * elapsed);
                    _tips[i].height = tipHeight;
                }

                SpectrumPointData p = spectrumPoints[i];
                int barIndex = p.SpectrumPointIndex;
                double xCoord = (_barWidth * barIndex) + (BarSpacing * barIndex) + 1 + _barWidth / 2;
                float y = CurrentSize.Height - tipHeight;

                var p1 = new PointF((float)xCoord, y - 0);
                var p2 = new PointF((float)xCoord, y + 1);

                graphics.DrawLine(tipPpen, p1, p2);
            }

            DrawTip(graphics, MousePoint);
        }

        private void DrawTip(Graphics graphics, Point? pos)
        {
            if (pos == null)
                return;

            Point p = (Point)pos;
            int height = CurrentSize.Height;

            int barIndex = (int)(((double)BarCount / CurrentSize.Width) * p.X);
            double xCoord = (_barWidth * barIndex) + (BarSpacing * barIndex) + 1 + _barWidth / 2;

            float y = (float)(Math.Floor((double)p.Y / (double)5) * 5);

            var p1 = new PointF((float)xCoord, y - 5);
            var p2 = new PointF((float)xCoord, y + 5);

            System.Drawing.Pen tipPen = new System.Drawing.Pen(System.Drawing.Brushes.WhiteSmoke, (float)_barWidth);

            graphics.DrawLine(tipPen, p1, p2);
        }

        protected override void UpdateFrequencyMapping()
        {
            _barWidth = Math.Max(((_currentSize.Width - (BarSpacing * (BarCount - 1))) / BarCount), 0.00001);
            if (base.SpectrumProvider != null)
                base.UpdateFrequencyMapping();
        }

        private bool UpdateFrequencyMappingIfNessesary(Size newSize)
        {
            if (newSize != CurrentSize)
            {
                CurrentSize = newSize;
                UpdateFrequencyMapping();
            }

            return newSize.Width > 0 && newSize.Height > 0;
        }

        private void PrepareGraphics(Graphics graphics, bool highQuality)
        {
            if (highQuality)
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.CompositingQuality = CompositingQuality.AssumeLinear;
                graphics.PixelOffsetMode = PixelOffsetMode.Default;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            }
            else
            {
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.None;
                graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            }
        }
    }
}
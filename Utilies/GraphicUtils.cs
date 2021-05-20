using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace Equalizer.Utils
{
    public class Drawing
    {
        public static System.Drawing.Point ToPoint(System.Windows.Point p)
        {
            return new System.Drawing.Point()
            {
                X = (int)p.X,
                Y = (int)p.Y
            };
        }
    }

    public class ColorUtils
    {
        public static Color ToDrawingColor(System.Windows.Media.Color c)
        {
            return Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static Color ToDrawingColor(System.Windows.Media.Brush b)
        {
            if (b == null)
                return Color.Transparent;

            return ToDrawingColor(((System.Windows.Media.SolidColorBrush)b).Color);
        }
    }
    public class BMP
    {
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }

        }
    }
}

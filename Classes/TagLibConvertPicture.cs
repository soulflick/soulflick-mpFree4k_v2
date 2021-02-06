using System;
using System.Linq;

namespace Classes
{
    public class TagLibConvertPicture
    {
        public static System.Windows.Media.Imaging.BitmapImage GetImageFromTag(TagLib.IPicture[] Pictures)
        {
            if (Pictures.Length == 0 || Pictures[0] == null)
            {
                return null;
            }

            byte[] raw = Pictures[0].Data.ToArray();
            System.Windows.Media.Imaging.BitmapImage _img = new System.Windows.Media.Imaging.BitmapImage();

            try
            {
                _img.BeginInit();
                _img.UriSource = null;
                _img.BaseUri = null;
                _img.StreamSource = new System.IO.MemoryStream(raw);
                _img.EndInit();
                _img.Freeze();
                return _img;
            }
            catch (NotSupportedException nsExc)
            {
                _img = null;
                return _img;
            }
            catch (Exception exc)
            {
                _img = null;
                return _img;
            }
        }
    }
}

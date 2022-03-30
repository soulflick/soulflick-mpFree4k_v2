
using Configuration;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpFree4k.Classes
{
    public class StandardImage
    {
        private static BitmapImage _defaultAlbumImage = null;

        public static void Reload()
        {
            string uri = @"pack://application:,,,/MpFree4k;component/Images/no_album_cover.jpg";

            if (UserConfig.Skin == Mpfree4k.Enums.SkinColors.Black_Smooth)
                uri = @"pack://application:,,,/MpFree4k;component/Images/no_album_cover_black.png";

            _defaultAlbumImage = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(uri, System.UriKind.Absolute));
        }

        public static BitmapImage DefaultAlbumImage
        {
            get
            {
                if (_defaultAlbumImage == null)
                    Reload();

                return _defaultAlbumImage;
            }
        }
    }
}


using Configuration;
using System.Windows.Media.Imaging;

namespace MpFree4k.Classes
{
    public class StandardImage
    {
        private static BitmapImage _defaultAlbumImage = null;

        public static BitmapImage GetImage(string resource_string)
        {
            return new BitmapImage(new System.Uri(resource_string, System.UriKind.Absolute));
        }

        public static void Reload()
        {
            string uri = @"pack://application:,,,/MpFree4k;component/Images/no_album_cover.jpg";

            if (UserConfig.Skin == Mpfree4k.Enums.SkinColors.Black_Smooth)
                uri = @"pack://application:,,,/MpFree4k;component/Images/no_album_cover_black.png";

            _defaultAlbumImage = new BitmapImage(new System.Uri(uri, System.UriKind.Absolute));
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

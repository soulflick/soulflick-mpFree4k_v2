using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Models
{
    public class SimpleAlbumItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public static BitmapImage DefaultAlbumImage = null;

        public int id = 0;
        public uint Year { get; set; }
        public string Artist { get; set; }
        public string AlbumLabel { get; set; }
        public string Genre { get; set; }
        public int TrackCount { get; set; }

        public bool HasImage = false; 

        public string[] Tracks { get; set; }

        private ImageSource _image = null;

        public bool HasAlbumImage => _image != null;

        public ImageSource AlbumImage
        {
            get => _image;
            set
            {
                _image = value;
                Raise(nameof(AlbumImage));
            }
        }

        public SimpleAlbumItem()
        {
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                Raise(nameof(IsVisible));
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                Raise(nameof(IsSelected));
            }
        }
    }
}

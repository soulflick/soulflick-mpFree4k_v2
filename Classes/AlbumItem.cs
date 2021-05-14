using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Classes
{

    public class AlbumItem : INotifyPropertyChanged
    {
        public static BitmapImage DefaultAlbumImage = null;
        public uint Year { get; set; }
        public string Artist { get; set; }
        public List<string> AllArtist { get; set; }
        public string Album { get; set; }
        public string AlbumLabel { get; set; }
        public int Count { get; set; }
        public string Genre { get; set; }
        public int TrackCount { get; set; }

        private List<string> _tracks = new List<string>();
        public List<string> Tracks
        {
            get => _tracks;
            set => _tracks = value;
        }

        private ImageSource _image = null;

        private bool _hasAlbumImage = false;
        public bool HasAlbumImage
        {
            get => _hasAlbumImage;
            set
            {
                _hasAlbumImage = value;
                OnPropertyChanged("HasAlbumImage");
            }
        }
        public ImageSource AlbumImage
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged("AlbumImage");
            }
        }

        public AlbumItem()
        {
            if (AlbumItem.DefaultAlbumImage == null)
            {
                string uri = @"pack://application:,,,/" +
                System.Reflection.Assembly.GetCallingAssembly().GetName().Name +
                ";component/" + "Images/no_album_cover.jpg";

                DefaultAlbumImage = new System.Windows.Media.Imaging.BitmapImage(
                new System.Uri(uri, System.UriKind.Absolute));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool _isSpecialVisible = false;
        public bool IsSpecialVisible
        {
            get => _isSpecialVisible;
            set
            {
                _isSpecialVisible = value;
                OnPropertyChanged("IsSpecialVisible");
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace Classes
{
    public class ArtistItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _artists = "-";
        public string Artists
        {
            get => _artists;
            set
            {
                _artists = value;
                OnPropertyChanged("Artists");
            }
        }

        private string _artistLabel = "-";
        public string ArtistLabel
        {
            get => _artistLabel;
            set
            {
                _artistLabel = value;
                OnPropertyChanged("ArtistLabel");
            }
        }
        private int _albumCount = 0;
        public int AlbumCount
        {
            get => _albumCount;
            set
            {
                _albumCount = value;
                OnPropertyChanged("AlbumCount");
            }
        }
        public int _trackCount = 0;
        public int TrackCount
        {
            get => _trackCount;
            set
            {
                _trackCount = value;
                OnPropertyChanged("TrackCount");
            }
        }

        public int year = 0;
        public int Year
        {
            get => year;
            set
            {
                year = value;
                OnPropertyChanged("Year");
            }
        }

        private List<SimpleAlbumItem> _albums = new List<SimpleAlbumItem>();
        public List<SimpleAlbumItem> Albums
        {
            get => _albums;
            set
            {
                _albums = value;
                OnPropertyChanged("Albums");
            }
        }
        
        public bool HasAlbumImage => _firstAlbum != null;

        private ImageSource _firstAlbum = null;
        public ImageSource FirstAlbum
        {
            get => _firstAlbum;
            set
            {
                _firstAlbum = value;
                OnPropertyChanged("FirstAlbum");
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

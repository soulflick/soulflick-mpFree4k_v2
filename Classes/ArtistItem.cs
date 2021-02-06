using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace Classes
{
    public class ArtistItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _artists = "-";
        public string Artists
        {
            get { return _artists; }
            set
            {
                _artists = value;
                OnPropertyChanged("Artists");
            }
        }

        private string _artistLabel = "-";
        public string ArtistLabel
        {
            get { return _artistLabel; }
            set
            {
                _artistLabel = value;
                OnPropertyChanged("ArtistLabel");
            }
        }
        private int _albumCount = 0;
        public int AlbumCount
        {
            get { return _albumCount; }
            set
            {
                _albumCount = value;
                OnPropertyChanged("AlbumCount");
            }
        }
        public int _trackCount = 0;
        public int TrackCount
        {
            get { return _trackCount; }
            set
            {
                _trackCount = value;
                OnPropertyChanged("TrackCount");
            }
        }

        public int year = 0;
        public int Year
        {
            get { return year; }
            set
            {
                year = value;
                OnPropertyChanged("Year");
            }
        }

        private List<SimpleAlbumItem> _albums = new List<SimpleAlbumItem>();
        public List<SimpleAlbumItem> Albums
        {
            get { return _albums; }
            set
            {
                _albums = value;
                OnPropertyChanged("Albums");
            }
        }
        
        public bool HasAlbumImage
        {
            get { return _firstAlbum != null;  }
        }

        private ImageSource _firstAlbum = null;
        public ImageSource FirstAlbum
        {
            get { return _firstAlbum; }
            set
            {
                _firstAlbum = value;
                OnPropertyChanged("FirstAlbum");
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
    }
}

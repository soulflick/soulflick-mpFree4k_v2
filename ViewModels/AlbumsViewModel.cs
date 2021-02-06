using MpFree4k;
using MpFree4k.Enums;
using MpFree4k.Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace ViewModels
{
    public class AlbumsViewModel : INotifyPropertyChanged
    {
        Dispatcher currentDispatcher = null;
        public AlbumsViewModel()
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
        }
        private MediaLibrary _mediaLibrary = null;
        public MediaLibrary MediaLibrary
        {
            get { return _mediaLibrary; }
            set
            {
                _mediaLibrary = value;
                _mediaLibrary.PropertyChanged -= Library_PropertiesChanged;
                _mediaLibrary.PropertyChanged += Library_PropertiesChanged;

                OnPropertyChanged("Albums");
            }
        }

        private void Library_PropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Albums")
            {
                MainWindow.mainDispatcher.Invoke(() => OnPropertyChanged("Albums"));

                UpdateAmount();
            }
            else if (e.PropertyName == "IndexAlbums")
            {
                OnPropertyChanged(e.PropertyName);
            }
        }

        public void UpdateAmount()
        {
            double duration = MediaLibrary.Files.Where(f => Albums.Any(a => a.IsVisible && a.Tracks.Contains(f.Path))).Sum(f => f.Mp3Fields.DurationValue);
            TimeSpan span = TimeSpan.FromSeconds(duration);
            MainWindow._singleton.SetAmounts(Albums.Count(a => a.IsVisible), span);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private List<Classes.AlbumItem> _albums = new List<Classes.AlbumItem>();
        public List<Classes.AlbumItem> Albums
        {
            get {
                if (MediaLibrary == null)
                    return null;
                return MediaLibrary.Albums; }
            set
            {
                _albums = value;
                UpdateAmount();
                OnPropertyChanged("Albums");
            }
        }

        public void OrderBy(AlbumOrderType orderType)
        {
            switch (orderType)
            {
                case AlbumOrderType.Album:
                    this.MediaLibrary.Albums = MediaLibrary.Albums.OrderBy(a => a.Album).ToList();
                    break;
                case AlbumOrderType.Artist:
                    this.MediaLibrary.Albums = MediaLibrary.Albums.OrderBy(a => a.Artist).ToList();
                    break;
                case AlbumOrderType.Genre:
                    this.MediaLibrary.Albums = MediaLibrary.Albums.OrderBy(a => a.Genre).ToList();
                    break;
                case AlbumOrderType.Tracks:
                    this.MediaLibrary.Albums = MediaLibrary.Albums.OrderByDescending(a => a.TrackCount).ToList();
                    break;
                case AlbumOrderType.Year:
                    this.MediaLibrary.Albums = MediaLibrary.Albums.OrderByDescending(a => a.Year).ToList();
                    break;
                default:
                    this.MediaLibrary.Albums = MediaLibrary.Albums.OrderBy(a => a.Album).ToList();
                    break;
            }

            this.MediaLibrary.Refresh(MediaLevel.Albums);
        }
    }
}

using MpFree4k;
using Mpfree4k.Enums;
using Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Models;

namespace ViewModels
{
    public class AlbumsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Raise(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public static AlbumsViewModel Instance;

        public AlbumsViewModel() => Instance = this;
        
        private MediaLibrary _mediaLibrary = null;
        public MediaLibrary MediaLibrary
        {
            get => _mediaLibrary;
            set
            {
                _mediaLibrary = value;
                _mediaLibrary.PropertyChanged -= Library_PropertiesChanged;
                _mediaLibrary.PropertyChanged += Library_PropertiesChanged;

                Raise("Albums");
            }
        }

        public void UpdateAmount()
        {
            double duration = MediaLibrary.Files.Where(f => Albums.Any(a => a.IsVisible && a.Tracks.Contains(f.Path))).Sum(f => f.Mp3Fields.DurationValue);
            TimeSpan span = TimeSpan.FromSeconds(duration);
            MainWindow.Instance.SetAmounts(Albums.Count(a => a.IsVisible), span);
        }

        private List<AlbumItem> _albums = new List<AlbumItem>();
        public List<AlbumItem> Albums
        {
            get => MediaLibrary?.Albums;
            set
            {
                _albums = value;
                UpdateAmount();
                Raise(nameof(Albums));
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

            MediaLibrary.Refresh(MediaLevel.Albums);
        }

        private void Library_PropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Albums))
            {
                MainWindow.mainDispatcher.Invoke(() => Raise(nameof(Albums)));

                UpdateAmount();
            }
            else if (e.PropertyName == "IndexAlbums")
            {
                Raise(e.PropertyName);
            }
        }
    }
}

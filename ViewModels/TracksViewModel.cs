using MpFree4k;
using Mpfree4k.Enums;
using Layers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Models;

namespace ViewModels
{
    public class TracksViewModel : INotifyPropertyChanged
    {
        public static TracksViewModel Instance = null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public TracksViewModel() => Instance = this;

        private MediaLibrary _mediaLibrary = null;
        public MediaLibrary MediaLibrary
        {
            get => _mediaLibrary;
            set
            {
                _mediaLibrary = value;
                _mediaLibrary.PropertyChanged -= Library_PropertiesChanged;
                _mediaLibrary.PropertyChanged += Library_PropertiesChanged;

                Raise(nameof(Tracks));
            }
        }


        private List<FileViewInfo> _tracks = new List<FileViewInfo>();
        public List<FileViewInfo> Tracks
        {
            get => MediaLibrary?.Files.Where(f => f.IsVisible)?.ToList();
            set
            {
                _tracks = value;
                Raise(nameof(Tracks));
            }
        }

        private void Library_PropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Tracks))
                MainWindow.mainDispatcher.Invoke(() => Raise("Tracks"));
        }

        public void OrderBy(TrackOrderType orderType)
        {
            switch (orderType)
            {
                case TrackOrderType.Standard:
                    MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.Album).ThenBy(b => b.Mp3Fields.Track).ThenBy(c => c.Mp3Fields.Artists).ToList();
                    break;
                case TrackOrderType.TrackName:
                    MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.Title).ToList();
                    break;
                case TrackOrderType.Album:
                    MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.Album).ToList();
                    break;
                case TrackOrderType.Artist:
                    MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.AlbumArtists).ToList();
                    break;
                case TrackOrderType.Genre:
                    MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.Genres).ToList();
                    break;
                case TrackOrderType.Year:
                    MediaLibrary.Files = MediaLibrary.Files.OrderByDescending(a => a.Mp3Fields.Year).ToList();
                    break;
                default:
                    MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.Title).ToList();
                    break;
            }

            MediaLibrary.Refresh(MediaLevel.Tracks);
        }
    }
}

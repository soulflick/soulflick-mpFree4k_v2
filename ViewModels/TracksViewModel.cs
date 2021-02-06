using MpFree4k;
using MpFree4k.Enums;
using MpFree4k.Layers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace ViewModels
{
    public class TracksViewModel : INotifyPropertyChanged
    {
        public static TracksViewModel _singleton = null;
        Dispatcher currentDispatcher = null;
        public TracksViewModel()
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
            _singleton = this;
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

                OnPropertyChanged("Tracks");
            }
        }

        private void Library_PropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tracks")
                MainWindow.mainDispatcher.Invoke(() => OnPropertyChanged("Tracks"));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private List<Classes.FileViewInfo> _tracks = new List<Classes.FileViewInfo>();
        public List<Classes.FileViewInfo> Tracks
        {
            get
            {
                if (MediaLibrary == null)
                    return null;

                return MediaLibrary.Files.Where(f => f.IsVisible).ToList();
            }
            set
            {
                _tracks = value;
                OnPropertyChanged("Tracks");
            }
        }

        public void OrderBy(TrackOrderType orderType)
        {
            switch (orderType)
            {
                case TrackOrderType.Standard:
                    this.MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.Album).ThenBy(b => b.Mp3Fields.Track).ThenBy(c => c.Mp3Fields.Artists).ToList();
                    break;
                case TrackOrderType.TrackName:
                    this.MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.Title).ToList();
                    break;
                case TrackOrderType.Album:
                    this.MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.Album).ToList();
                    break;
                case TrackOrderType.Artist:
                    this.MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.AlbumArtists).ToList();
                    break;
                case TrackOrderType.Genre:
                    this.MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.Genres).ToList();
                    break;
                case TrackOrderType.Year:
                    this.MediaLibrary.Files = MediaLibrary.Files.OrderByDescending(a => a.Mp3Fields.Year).ToList();
                    break;
                default:
                    this.MediaLibrary.Files = MediaLibrary.Files.OrderBy(a => a.Mp3Fields.Title).ToList();
                    break;
            }

            this.MediaLibrary.Refresh(MediaLevel.Tracks);
        }
    }
}

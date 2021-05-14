using MpFree4k.Enums;
using MpFree4k.Layers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace ViewModels
{
    public class ArtistsViewModel : INotifyPropertyChanged
    {
        public ArtistsViewModel()
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
        }

        Dispatcher currentDispatcher = null;
        private MediaLibrary _mediaLibrary = null;
        public MediaLibrary MediaLibrary
        {
            get => _mediaLibrary;
            set
            {
                _mediaLibrary = value;
                _mediaLibrary.PropertyChanged -= Library_PropertiesChanged;
                _mediaLibrary.PropertyChanged += Library_PropertiesChanged;

                //OnPropertyChanged("Artists");
            }
        }

        private void Library_PropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Artists")
            {
                OnPropertyChanged("Artists");
                //currentDispatcher.Invoke(new Action(() => OnPropertyChanged("Artists")), DispatcherPriority.Render);
                //MainWindow.mainDispatcher.BeginInvoke(new Action(() => OnPropertyChanged("Artists")));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private List<Classes.ArtistItem> _artists = new List<Classes.ArtistItem>();
        public List<Classes.ArtistItem> Artists
        {
            get
            {
                if (MediaLibrary == null)
                    return null;

                return MediaLibrary.Artists;
            }
            set
            {
                _artists = value;
                //OnPropertyChanged("Artists");
            }
        }


        public void OrderBy(ArtistOrderType orderType)
        {
            switch (orderType)
            {
                case ArtistOrderType.MostContent:
                    this.MediaLibrary.Artists = MediaLibrary.Artists.OrderByDescending(x => x.TrackCount).ToList();
                    break;
                case ArtistOrderType.Year:
                    this.MediaLibrary.Artists = MediaLibrary.Artists.OrderByDescending(x => x.Albums.Max(a => a.Year)).ToList();
                    break;
                default:
                    this.MediaLibrary.Artists = MediaLibrary.Artists.OrderBy(a => a.Artists).ToList();
                    break;
            }

            this.MediaLibrary.Refresh(MediaLevel.Artists);
            this.OnPropertyChanged("IndexArtists");
        }
    }
}

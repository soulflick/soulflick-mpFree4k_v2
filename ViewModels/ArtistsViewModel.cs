using Mpfree4k.Enums;
using Layers;
using Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ViewModels
{
    public class ArtistsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        protected void ReRaise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public ArtistsViewModel() { ; }

        private MediaLibrary _mediaLibrary = null;
        public MediaLibrary MediaLibrary
        {
            get => _mediaLibrary;
            set
            {
                _mediaLibrary = value;
                _mediaLibrary.PropertyChanged -= Library_PropertiesChanged;
                _mediaLibrary.PropertyChanged += Library_PropertiesChanged;
            }
        }

        private List<ArtistItem> _artists = new List<ArtistItem>();
        public List<ArtistItem> Artists
        {
            get => MediaLibrary?.Artists;
            set  => _artists = value;
        }

        public void OrderBy(ArtistOrderType orderType)
        {
            switch (orderType)
            {
                case ArtistOrderType.MostContent:
                    MediaLibrary.Artists = MediaLibrary.Artists.OrderByDescending(x => x.TrackCount).ToList();
                    break;
                case ArtistOrderType.Year:
                    MediaLibrary.Artists = MediaLibrary.Artists.OrderByDescending(x => x.Albums.Max(a => a.Year)).ToList();
                    break;
                default:
                    MediaLibrary.Artists = MediaLibrary.Artists.OrderBy(a => a.Artists).ToList();
                    break;
            }

            MediaLibrary.Refresh(MediaLevel.Artists);
            ReRaise("IndexArtists");
        }

        private void Library_PropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Artists")
            {
                ReRaise("Artists");
            }
        }
    }
}

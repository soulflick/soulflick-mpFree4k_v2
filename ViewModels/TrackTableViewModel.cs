using Classes;
using MpFree4k.Layers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace MpFree4k.ViewModels
{
    public class TrackTableViewModel : INotifyPropertyChanged
    {
        public TrackTableViewModel()
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
            Instance = this;
        }

        public static TrackTableViewModel Instance = null;
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

        public IEnumerable<FileViewInfo> LibraryTracks { get => MediaLibrary.Files; }

        private List<FileViewInfo> _tracks = new List<FileViewInfo>();
        public List<FileViewInfo> Tracks
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

    }
}

using Layers;
using Models;
using MpFree4k;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ViewModels
{
    public class TrackTableViewModel : INotifyPropertyChanged
    {
        public static TrackTableViewModel Instance = null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void Raise(string info) => PropertyChanged(this, new PropertyChangedEventArgs(info));
        protected void ReRaise(string info) => PropertyChanged(this, new PropertyChangedEventArgs(info));

        public TrackTableViewModel() => Instance = this;

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

        private bool _showPathInLibrary = false;
        public bool ShowPathInLibrary
        {
            get => _showPathInLibrary;
            set
            {
                _showPathInLibrary = value;
                Raise(nameof(ShowPathInLibrary));

                if (DataGrid != null)
                    _dataGrid.Columns[1].Visibility = ShowPathInLibrary ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            }
        }

        private System.Windows.Controls.DataGrid _dataGrid = null;
        public System.Windows.Controls.DataGrid DataGrid
        {
            get => _dataGrid;
            set
            {
                _dataGrid = value;
                _dataGrid.Columns.FirstOrDefault(c => (string)c.Header == "File").Visibility = 
                    ShowPathInLibrary ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public IEnumerable<FileViewInfo> LibraryTracks => MediaLibrary.Files;

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
                Raise(nameof(Tracks));
            }
        }

        private void Library_PropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tracks")
                MainWindow.mainDispatcher.Invoke(() => ReRaise("Tracks"));
        }
    }
}

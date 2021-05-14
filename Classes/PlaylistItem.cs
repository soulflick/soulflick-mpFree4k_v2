using System;
using System.ComponentModel;

namespace MpFree4k.Classes
{
    [Serializable]
    public class PlaylistItem : INotifyPropertyChanged
    {

        public PlaylistItem()
        {
            this.uniqueID = PlaylistHelpers.CreateUniqueID();
        }

        private bool _dragOver = false;

        [field: NonSerialized]
        public bool DragOver
        {
            get => _dragOver;
            set { _dragOver = value; NotifyPropertyChanged("DragOver"); }
        }

        private bool _isMouseOver = false;
        public bool IsMouseOver
        {
            get => _isMouseOver;
            set { _isMouseOver = value; NotifyPropertyChanged("IsMouseOver"); }
        }

        public bool _isPlaying = false;
        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; NotifyPropertyChanged("IsPlaying"); }
        }

        public string uniqueID = "";

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        [field: NonSerialized]
        public void NotifyPropertyChanged(String info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public int _position = 0;
        public int Position
        {
            get => _position;
            set
            {
                _position = value;
                NotifyPropertyChanged("Position");
            }
        }

        private string _trackNumber = string.Empty;
        public string TrackNumber
        {
            get => _trackNumber;
            set
            {
                if (Convert.ToInt32(value) < 10) _trackNumber = "  " + value;
                else _trackNumber = value;
                _trackNumber += '.';
                NotifyPropertyChanged("TrackNumber");
            }
        }

        private string _title = string.Empty;
        public string Title 
        { 
            get => _title;
            set { _title = value; NotifyPropertyChanged("Title"); } 
        }

        private string _trackname = string.Empty;
        public string TrackName 
        { 
            get => _trackname;
            set { _trackname = value; NotifyPropertyChanged("TrackName"); } 
        }

        private string _tracklabel = string.Empty;
        public string TrackLabel 
        { 
            get => _tracklabel;
            set { _tracklabel = value; NotifyPropertyChanged("TrackLabel"); } 
        }

        private string _path = string.Empty;
        public string Path 
        { 
            get => _path;
            set { _path = value; NotifyPropertyChanged("FileName"); } 
        }

        private uint _track = 0;
        public uint Track
        {
            get => _track;
            set { _track = value; NotifyPropertyChanged("Track"); }
        }

        private string _artists = string.Empty;
        public string Artists 
        { 
            get => _artists;
            set { _artists = value; NotifyPropertyChanged("Artists"); } 
        }

        private string _album = string.Empty;
        public string Album
        { 
            get => _album;
            set { _album = value; NotifyPropertyChanged("Album"); } 
        }

        private string _year = string.Empty;
        public string Year 
        { 
            get => _year;
            set { _year = value; NotifyPropertyChanged("Year"); } 
        }

        [field: NonSerialized]
        private System.Windows.Media.Imaging.BitmapImage _image = null;

        [field: NonSerialized]
        public System.Windows.Media.Imaging.BitmapImage Image
        {
            get => _image;
            set { _image = value; NotifyPropertyChanged("Image"); }
        }

        private string _duration = string.Empty;
        public string Duration
        {
            get => _duration;
            set { _duration = value; NotifyPropertyChanged("Duration"); }
        }

        public static string getNameFromLabel(string str)
        {
            if (str.IndexOf("-") < 0) return string.Empty;
            string name = str.Substring(str.IndexOf("-") + 2);
            return name;
        }

        private string _toolTip = string.Empty;
        public string ToolTip
        {
            get =>_toolTip;
            set => _toolTip = value;
        }
    }
}

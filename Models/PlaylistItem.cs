using Classes;
using System;
using System.ComponentModel;

namespace Models
{
    [Serializable]
    public class PlaylistInfo : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public PlaylistInfo() => uniqueID = PlaylistHelpers.CreateUniqueID();

        private bool _dragOver = false;

        [field: NonSerialized]
        public bool DragOver
        {
            get => _dragOver;
            set { _dragOver = value; Raise(nameof(DragOver)); }
        }

        private bool _isMouseOver = false;
        public bool IsMouseOver
        {
            get => _isMouseOver;
            set { _isMouseOver = value; Raise(nameof(IsMouseOver)); }
        }

        public bool _isPlaying = false;
        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; Raise(nameof(IsPlaying)); }
        }

        public string uniqueID = "";


        public int _position = 0;
        public int Position
        {
            get => _position;
            set
            {
                _position = value;
                Raise(nameof(Position));
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
                Raise(nameof(TrackNumber));
            }
        }

        private string _title = string.Empty;
        public string Title 
        { 
            get => _title;
            set { _title = value; Raise(nameof(Title)); } 
        }

        private string _trackname = string.Empty;
        public string TrackName 
        { 
            get => _trackname;
            set { _trackname = value; Raise(nameof(TrackName)); } 
        }

        private string _tracklabel = string.Empty;
        public string TrackLabel 
        { 
            get => _tracklabel;
            set { _tracklabel = value; Raise(nameof(TrackLabel)); } 
        }

        private string _path = string.Empty;
        public string Path 
        { 
            get => _path;
            set { _path = value; Raise("FileName"); } 
        }

        private uint _track = 0;
        public uint Track
        {
            get => _track;
            set { _track = value; Raise(nameof(Track)); }
        }

        private string _artists = string.Empty;
        public string Artists 
        { 
            get => _artists;
            set { _artists = value; Raise(nameof(Artists)); } 
        }

        private string _album = string.Empty;
        public string Album
        { 
            get => _album;
            set { _album = value; Raise(nameof(Album)); } 
        }

        private string _year = string.Empty;
        public string Year 
        { 
            get => _year;
            set { _year = value; Raise(nameof(Year)); } 
        }

        [field: NonSerialized]
        private System.Windows.Media.Imaging.BitmapImage _image = null;

        public System.Windows.Media.Imaging.BitmapImage Image
        {
            get => _image;
            set { _image = value; Raise(nameof(Image)); }
        }

        private string _duration = string.Empty;
        public string Duration
        {
            get => _duration;
            set { _duration = value; Raise(nameof(Duration)); }
        }

        private string _toolTip = string.Empty;
        public string ToolTip
        {
            get =>_toolTip;
            set => _toolTip = value;
        }
    }
}

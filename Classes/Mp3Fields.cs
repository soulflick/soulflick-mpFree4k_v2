using System;
using System.ComponentModel;
using System.Linq;

namespace Classes
{
    [Serializable()]
    public class Mp3Fields : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //public TagLib.File _Handle { get; set; }
        public bool HasChanged = false;

        private bool _consider = false;
        public bool Consider { get { return _consider; } set { _consider = value; } }

        private string _filename = string.Empty;
        public string FileName { get { return _filename; } set {
            if (_filename == value) return;
            _filename = value; 
            HasChanged = true; 
            
        } }
        private string _album = string.Empty;
        public string Album { get { return _album; } set {
            string val = value.Replace("\n", " ");
            if (_album == val) return;
            _album = val; HasChanged = true; } }

        private uint _disc = 1;
        public uint Disc { get { return _disc; } set {
            if (_disc == value) return;
            _disc = value; HasChanged = true; } }
        
        private uint _discCount = 1;
        public uint DiscCount { get { return _discCount; } set {
            if (_discCount == value) return;
            _discCount = value; HasChanged = true; } }
        
        private uint _year = 0;
        public uint Year { get { return _year; } set {
            if (_year == value) return;
            _year = value; HasChanged = true; } }
        
        private string _title = string.Empty;
        public string Title { get { return _title; } set {
                string val = value.Replace("\n", " ");
                if (_title == val) return;
            _title = val; HasChanged = true; } }
        
        private uint _track = 0;
        public uint Track { get { return _track; } set {
            if (_track == value) return;
            _track = value; HasChanged = true; } }
        
        private uint _trackCount = 0;
        public uint TrackCount { get { return _trackCount; } set {
            if (_trackCount == value) return;
            _trackCount = value; HasChanged = true; } }
        
        private string _artists = string.Empty;
        public string Artists { get { return _artists; } set {
                string val = value.Replace("\n", " ");
                if (_artists == val) return;
            _artists = val; HasChanged = true; } }
        
        private string _albumArtists = string.Empty;
        public string AlbumArtists { get { return _albumArtists; } set {
                string val = value.Replace("\n", " ");
                if (_albumArtists == val) return;
            _albumArtists = val; HasChanged = true; } }
        
        private string _performers = string.Empty;
        public string Performers { get { return _performers; } set {
                string val = value.Replace("\n", " ");
                if (_performers == val) return;
            _performers = val; HasChanged = true; } }
        
        private string _composers = string.Empty;
        public string Composers { get { return _composers; } set {
            if (_composers == value) return;
            _composers = value; HasChanged = true; } }
        
        private string _copyright = string.Empty;
        public string Copyright { get { return _copyright; } set {
            if (_copyright == value) return;
            _copyright = value; HasChanged = true; } }
        
        private string _comment = string.Empty;
        public string Comment { get { return _comment; } set {
            if (_comment == value) return;
            _comment = value; HasChanged = true; } }
        
        private string _genres = string.Empty;
        public string Genres { get { return _genres; } set {
            if (_genres == value) return;
            _genres = value.Replace("\n", ", "); HasChanged = true; } }

        private string _bitrate = string.Empty;
        public string Bitrate
        {
            get { return _bitrate; }
            set
            {
                if (_bitrate == value) return;
                BitrateValue = Convert.ToInt64(value);
                _bitrate = value; HasChanged = true;
            }
        }

        public long BitrateValue { get; set; } = 0;

        private double _durationValue = 0;
        public double DurationValue
        {
            get { return _durationValue; }
            set
            {
                _durationValue = value;
                OnPropertyChanged("DurationValue");
            }
        }

        private string _duration = string.Empty;
        public string Duration
        {
            get {
                return 
                    _duration; }
            set
            {
                if (_duration == value) return;

                _duration = value.TrimStart("0:".ToCharArray());
                if (_duration.Length == 1)
                    _duration = '0' + Duration;
                if (_duration.IndexOf(':') < 0)
                    _duration = "00:" + _duration;

                string dur = _duration;

                double secs = 0, mins = 0, hours = 0;
                int idx = dur.IndexOf(':');

                int idxCount = dur.Count(d => d == ':');
                if (idxCount < 1)
                    secs = Convert.ToDouble(dur);
                else if (idxCount == 1)
                {
                    mins = Convert.ToDouble(dur.Substring(0, idx));
                    secs = Convert.ToDouble(dur.Substring(idx + 1));
                }
                else
                {
                    int idx2 = dur.LastIndexOf(':');
                    hours = Convert.ToDouble(dur.Substring(0, idx));
                    mins = Convert.ToDouble(dur.Substring(idx+1, dur.Length - idx2 - 1));
                    secs = Convert.ToDouble(dur.Substring(idx2 + 1));
                }

                secs += hours * 3600;
                secs += mins * 60;
                DurationValue = secs;

                HasChanged = true;
            }
        }
    }
}

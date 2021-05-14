using System;
using System.ComponentModel;

namespace Classes
{
    public class FileViewInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public bool IsItemChecked { get; set; }

        bool _isPlaying = false;
        public bool IsPlaying 
        { 
            get => _isPlaying; 
            set { _isPlaying = value; Raise(nameof(IsPlaying)); } 
        }

        bool _handleError = false;
        public bool HandleError 
        { 
            get => _handleError; 
            set { _handleError = value; Raise(nameof(HandleError)); } 
        }

        public bool HasChanged { get; set; }

        private bool _isSelected = false;
        public bool IsSelected 
        {
            get => _isSelected;
            set
            {
                _isSelected = value; Raise(nameof(IsSelected));
            }
        }

        private int _playcount = 0;
        public int PlayCount
        {
            get => _playcount;
            set
            {
                _playcount = value;
            }
        }

        private DateTime _lastPlayed = new DateTime();
        public DateTime LastPlayed
        {
            get => _lastPlayed;
            set
            {
                _lastPlayed = value;
            }
        }

        private int _number = 0;
        public int Number
        {
            get => _number;
            set
            {
                _number = value;
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                Raise(nameof(IsVisible));
            }
        }
        public string Title { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public string Label { get; set; }
        public TagLib.File _Handle { get; set; }

        private System.Windows.Media.Imaging.BitmapImage _image = null;
        public System.Windows.Media.Imaging.BitmapImage Image
        {
            get => _image;
            set { _image = value; Raise(nameof(Image)); }
        }

        private System.Windows.Media.Imaging.BitmapImage _trackImage = null;
        public System.Windows.Media.Imaging.BitmapImage TrackImage
        {
            get => _trackImage;
            set { _trackImage = value; Raise(nameof(TrackImage)); }
        }

        public bool HasTrackImage =>_trackImage != null;
        private Mp3Fields _mp3Fields = new Mp3Fields();
        public Mp3Fields Mp3Fields 
        { 
            get => _mp3Fields;
            set 
            {
                _mp3Fields = value;
            }
        }

        public FileViewInfo()
        {
            ;
        }
        public FileViewInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Label = "error";
                return;
            }

            Path = path;
            Label = path.Substring(path.LastIndexOf(@"\") + 1);

            try
            {
                FileViewInfoHelpers.CreateFileHandle(this);
            }
            catch
            {
                HandleError = true;
            }
        }
    }
}

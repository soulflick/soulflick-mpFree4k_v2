using System;
using System.ComponentModel;

namespace Classes
{
    public class FileViewInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        bool _isItemChecked = false;
        public bool IsItemChecked { get; set; }

        bool _isPlaying = false;
        public bool IsPlaying { get { return _isPlaying; } set { _isPlaying = value; NotifyPropertyChanged("IsPlaying"); } }

        bool _handleError = false;
        public bool HandleError { get { return _handleError; } set { _handleError = value; NotifyPropertyChanged("HandleError"); } }
        public bool HasChanged { get; set; }

        private bool _isSelected = false;
        public bool IsSelected { get { return _isSelected; }
            set
            {
                _isSelected = value; NotifyPropertyChanged("IsSelected");
            }
        }

        private int _playcount = 0;
        public int PlayCount
        {
            get
            {
                return _playcount;
            }
            set
            {
                _playcount = value;
            }
        }

        private DateTime _lastPlayed = new DateTime();
        public DateTime LastPlayed
        {
            get
            {
                return _lastPlayed;
            }
            set
            {
                _lastPlayed = value;
            }
        }

        private int _number = 0;
        public int Number
        {
            get { return _number; }
            set
            {
                _number = value;
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                NotifyPropertyChanged("IsVisible");
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
            get { return _image; }
            set { _image = value; NotifyPropertyChanged("Image"); }
        }

        private System.Windows.Media.Imaging.BitmapImage _trackImage = null;
        public System.Windows.Media.Imaging.BitmapImage TrackImage
        {
            get { return _trackImage; }
            set { _trackImage = value; NotifyPropertyChanged("TrackImage"); }
        }

        public bool HasTrackImage { get { return _trackImage != null; } }
        private Mp3Fields _mp3Fields = new Mp3Fields();
        public Mp3Fields Mp3Fields { get { return _mp3Fields; } set { _mp3Fields = value; } }

        public FileViewInfo()
        {
            ;
        }
        public FileViewInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                this.Label = "error";
                return;
            }

            this.Path = path;
            this.Label = path.Substring(path.LastIndexOf(@"\") + 1);

            try
            {
                FileViewInfoHelpers.CreateFileHandle(this);
            }
            catch (Exception exc)
            {
                this.HandleError = true;
            }
        }

        

    }
}

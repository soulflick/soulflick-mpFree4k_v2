using MpFree4k.Classes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Models
{
    public class AlbumItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));


        public static BitmapImage DefaultAlbumImage => StandardImage.DefaultAlbumImage;
        public uint Year { get; set; }
        public string Artist { get; set; }
        public List<string> AllArtist { get; set; }
        public string Album { get; set; }
        public string AlbumLabel { get; set; }
        public int Count { get; set; }
        public string Genre { get; set; }
        public int TrackCount { get; set; }

        private List<string> _tracks = new List<string>();
        public List<string> Tracks
        {
            get => _tracks;
            set => _tracks = value;
        }

        private ImageSource _image = null;

        private bool _hasAlbumImage = false;
        public bool HasAlbumImage
        {
            get => _hasAlbumImage;
            set
            {
                _hasAlbumImage = value;
                Raise(nameof(HasAlbumImage));
            }
        }
        public ImageSource AlbumImage
        {
            get => _image;
            set
            {
                _image = value;
                Raise(nameof(AlbumImage));
            }
        }

        public AlbumItem()
        {
        }

        private bool _isSpecialVisible = false;
        public bool IsSpecialVisible
        {
            get => _isSpecialVisible;
            set
            {
                _isSpecialVisible = value;
                Raise(nameof(IsSpecialVisible));
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

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                Raise(nameof(IsSelected));
            }
        }

    }
}

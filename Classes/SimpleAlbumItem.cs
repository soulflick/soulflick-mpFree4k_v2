using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Classes
{

    public class SimpleAlbumItem : INotifyPropertyChanged
    {
        public static BitmapImage DefaultAlbumImage = null;
        public int id = 0;
        public uint Year { get; set; }
        public string Artist { get; set; }
        public string AlbumLabel { get; set; }
        public string Genre { get; set; }
        public int TrackCount { get; set; }

        public bool HasImage = false; 

        public string[] Tracks { get; set; }

        private ImageSource _image = null;

        public bool HasAlbumImage
        {
            get { return _image != null; }
        }

        public ImageSource AlbumImage
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                OnPropertyChanged("AlbumImage");
            }
        }

        public SimpleAlbumItem()
        {
            if (AlbumItem.DefaultAlbumImage == null)
                DefaultAlbumImage = new System.Windows.Media.Imaging.BitmapImage(
                new System.Uri(@"pack://application:,,,/" +
                System.Reflection.Assembly.GetCallingAssembly().GetName().Name +
                ";component/" + "Images/no_album_cover.jpg", System.UriKind.Absolute));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

    }
}

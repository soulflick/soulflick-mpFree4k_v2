using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Classes;
using Models;

namespace Controls
{
    public partial class Mp3TagView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public Mp3TagView() => InitializeComponent();

        private FileViewInfo _currentTag = null;
        public FileViewInfo CurrentTag
        {
            get => _currentTag;
            set
            {
                _currentTag = value;
                SetCurrentTag();
                Raise(nameof(CurrentTag));
                enableControls(true);
            }
        }

        public void enableControls(bool _enable)
        {
            tbAlbum.IsEnabled = _enable;
            tbArtists.IsEnabled = _enable;
            tbAlbumArtist.IsEnabled = _enable;
            tbComment.IsEnabled = _enable;
            tbComposers.IsEnabled = _enable;
            tbCopyright.IsEnabled = _enable;
            tbDisc.IsEnabled = _enable;
            tbDiscCount.IsEnabled = _enable;
            tbPerformers.IsEnabled = _enable;
            tbTitle.IsEnabled = _enable;
            tbTrack.IsEnabled = _enable;
            tbTrackCount.IsEnabled = _enable;
            tbYear.IsEnabled = _enable;
            tbGenres.IsEnabled = _enable;

            btnSaveMp3View.IsEnabled = _enable;       
            btnSaveImage.IsEnabled = _enable;
            btnImportImage.IsEnabled = _enable;
            btnRemoveImage.IsEnabled = _enable;

            lblFileName.IsEnabled = _enable;
        }

        private void btnSaveMp3View_Click(object sender, RoutedEventArgs e)
        {
            CurrentTag.Mp3Fields.HasChanged = true;
            CurrentTag.save();
        }

        public void SetCurrentTag()
        {
            DataContext = CurrentTag.Mp3Fields;
            AlbumImage.DataContext = CurrentTag.Image;
            AlbumImage.Source = CurrentTag.Image;
            FileProperties.SetInfo(CurrentTag);
        }

        private void btnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Imaging.BitmapImage img = CurrentTag.Image;
            if (img == null) return;

            ImageConnector.SaveImageToFile(img);
        }

        private void btnImportImage_Click(object sender, RoutedEventArgs e)
        {
            ImageConnector.ImportImage(CurrentTag);
            AlbumImage.Source = null;
            AlbumImage.Source = CurrentTag.Image;
        }

        private void btnRemoveImage_Click(object sender, RoutedEventArgs e)
        {
            AlbumImage.Source = null;
            AlbumImage.Source = CurrentTag.Image;
        }
    }
}

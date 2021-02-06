using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Classes;

namespace MpFree4k.Controls
{
    public partial class Mp3TagView : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private FileViewInfo _currentTag = null;
        public FileViewInfo CurrentTag
        {
            get { return _currentTag; }
            set
            {
                _currentTag = value;
                SetCurrentTag();
                OnPropertyChanged("CurrentTag");
                enableControls(true);
            }
        }
        public Mp3TagView()
        {
            InitializeComponent();
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

            btnDeepDetail.IsEnabled = _enable;
            btnSaveMp3View.IsEnabled = _enable;
            lblFileName.IsEnabled = _enable;

            btnSaveImage.IsEnabled = _enable;
            btnImportImage.IsEnabled = _enable;
            btnRemoveImage.IsEnabled = _enable;
            
                 
        }

        private void btnSaveMp3View_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentTag.Mp3Fields.HasChanged = true;
            this.CurrentTag.save();
        }

        public void SetCurrentTag()
        {
            this.DataContext = CurrentTag.Mp3Fields;
            this.AlbumImage.DataContext = CurrentTag.Image;
            this.AlbumImage.Source = CurrentTag.Image;
            this.FileProperties.SetInfo(CurrentTag);
        }

        private void btnDeepDetail_Click(object sender, RoutedEventArgs e)
        {
            
            //CurrentTag.save();
            //this.enableControls(false);

            //AllTagsView tView = new AllTagsView(CurrentTag);
            //Window wnd = new Window();
            //wnd.Background = System.Windows.Media.Brushes.White;
            //wnd.Content = tView;
            //wnd.Width = 505;
            //wnd.Height = 600;
            //wnd.ShowDialog();

            ////if (tView.dlgResult == System.Windows.Forms.DialogResult.OK)
            ////{
            ////    CurrentTag.saveHandle();
            ////}
            ////(Window.GetWindow(this) as MainWindow).fileListView.showMp3Info();
            
            //this.DataContext = null;
            //this.DataContext = CurrentTag.Mp3Fields;
            //this.enableControls(true);
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
            //CurrentTag.RemoveImage();
            AlbumImage.Source = null;
            AlbumImage.Source = CurrentTag.Image;
        }
    }
}

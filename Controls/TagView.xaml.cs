using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Classes;
using Models;

namespace Controls
{

    public partial class TagView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public TagView() => InitializeComponent();

        private FileViewInfo _currentTag = null;
        public FileViewInfo CurrentTag
        {
            get => _currentTag;
            set
            {
                if (value == null) return;
                _currentTag = value;
                SetCurrentTag();
                Raise(nameof(CurrentTag));
                enableControls(true);
            }
        }

        public List<FileViewInfo> AllFiles { get; set; }

        public void enableControls(bool _enable)
        {
            tbAlbum.IsEnabled = _enable;
            tbArtists.IsEnabled = false;
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
            lblFileName.IsEnabled = _enable;

            btnSaveImage.IsEnabled = _enable;
            btnImportImage.IsEnabled = _enable;
            btnRemoveImage.IsEnabled = _enable;


        }

        public void ClearInfo()
        {
            tbBitrate.Text = string.Empty;
            tbBitsPerSample.Text = string.Empty;
            tbChannels.Text = string.Empty;
            tbDescription.Text = string.Empty;
            tbDuration.Text = string.Empty;
            tbSampleRate.Text = string.Empty;
            tbCodecs.Text = string.Empty;
        }

        public void SetInfo(FileViewInfo info)
        {
            if (info._Handle == null)
            {

            }
            else
            {
                tbBitrate.Text = info._Handle.Properties.AudioBitrate.ToString();
                tbBitsPerSample.Text = info._Handle.Properties.BitsPerSample.ToString();
                tbChannels.Text = info._Handle.Properties.AudioChannels.ToString();
                tbDescription.Text = info._Handle.Properties.Description;
                tbDuration.Text = info._Handle.Properties.Duration.ToString(@"hh\:mm\:ss");
                tbSampleRate.Text = info._Handle.Properties.AudioSampleRate.ToString();
                tbCodecs.Text = string.Empty;

                foreach (var entry in info._Handle.Properties.Codecs)
                {
                    if (entry == null) continue;
                    tbCodecs.Text += entry.MediaTypes.ToString() + " ";
                }
            }
        }

        public void SetCurrentTag()
        {
            DataContext = CurrentTag.Mp3Fields;

            BitmapImage img = ImageConnector.GetImageFromFile(CurrentTag.Path);
            CurrentTag.Image = img;
            AlbumImage.DataContext = CurrentTag.Image;
            AlbumImage.Source = CurrentTag.Image;
            SetInfo(CurrentTag);
        }

        private void btnSaveMp3View_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentTag.Mp3Fields.HasChanged = true;
            bool success = this.CurrentTag.save();
            if (success)
            {
                MessageBox.Show("Tag for: " + CurrentTag.Path + " saved.");
            }
            else
            {
                MessageBox.Show("Could not save tag for: " + CurrentTag.Path);
            }
        }

        private void btnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage img = CurrentTag.Image;
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
            CurrentTag.RemoveImage();
            AlbumImage.Source = null;
            AlbumImage.Source = CurrentTag.Image;
        }

        private void btnGoTo_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentTag == null || string.IsNullOrEmpty(CurrentTag.Path)) return;

            try
            {
                Process.Start("explorer.exe",  Path.GetDirectoryName(CurrentTag.Path));
            }
            catch
            {
                MessageBox.Show("Cannot open storage location.");
            }

        }

        public void CancelEvents()
        {
            tbAlbumArtist.TextChanged -= tbAlbumArtist_TextChanged;
            tbAlbum.TextChanged -= tbAlbum_TextChanged;
            tbYear.TextChanged -= tbYear_TextChanged;
        }

        private void tbAlbumArtist_TextChanged(object sender, TextChangedEventArgs e)
        {
            string artist = (sender as TextBox).Text;
            AllFiles.Where(a => a.Path != CurrentTag.Path).ToList().ForEach(x => x.Mp3Fields.AlbumArtists = artist);
        }

        private void tbAlbum_TextChanged(object sender, TextChangedEventArgs e)
        {
            string album = (sender as TextBox).Text;
            AllFiles.Where(a => a.Path != CurrentTag.Path).ToList().ForEach(x => x.Mp3Fields.Album = album);
        }

        private void tbYear_TextChanged(object sender, TextChangedEventArgs e)
        {
            uint year = uint.Parse((sender as TextBox).Text);
            AllFiles.Where(a => a.Path != CurrentTag.Path).ToList().ForEach(x => x.Mp3Fields.Year = year);
        }

        private void tbPerformers_TextChanged(object sender, TextChangedEventArgs e)
        {
            string performers = (sender as TextBox).Text;
            AllFiles.Where(a => a.Path != CurrentTag.Path).ToList().ForEach(x => x.Mp3Fields.Performers = performers);
        }
    }
}

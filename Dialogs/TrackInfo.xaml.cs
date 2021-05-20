using Classes;
using Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Dialogs
{
    public class SimpleTrackItem
    {
        public string Name { get; set; }
        public string Length { get; set; }
        public int Track { get; set; }
    }

    public partial class TrackInfo : Window, INotifyPropertyChanged
    {
        public TrackInfo(FileViewInfo info, IEnumerable<FileViewInfo> collection)
        {
            InitializeComponent();

            this.collection = collection;
            DataContext = this;

            SetInfo(info);
        }

        private IEnumerable<FileViewInfo> collection;
        public FileViewInfo Info { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public IEnumerable<SimpleTrackItem> AlbumTracks { get; set; }

        public void SetInfo(FileViewInfo info)
        {
            Info = info;
            AlbumImage.Source = ImageConnector.GetImageFromFile(Info.Path);

            if (info._Handle != null)
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

            var tracks = Utilities.LibraryUtils.GetAlbumItems(info, collection);

            AlbumTracks = from track in tracks
                            select new SimpleTrackItem
                            {
                                Length = track.Mp3Fields.Duration,
                                Name = track.Title,
                                Track = (int)track.Mp3Fields.Track
                            };

        }

        private void Path_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", Path.GetDirectoryName(Info.Path));
            }
            catch
            {
                MessageBox.Show("Cannot open storage location.");
            }
        }
    }
}

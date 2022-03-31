using Classes;
using Models;
using MpFree4k.Classes;
using MpFree4k.Utilities;
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
        public double DurationValue { get; set; }
        public string Path { get; set; }
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

        public uint TrackCount
        {
            get
            {
                uint count = Info?.Mp3Fields.TrackCount ?? 0;
                if (count == 0) count = (uint)(AlbumTracks?.Count() ?? 0);
                int max_track = AlbumTracks.Max(t => t.Track);
                if (max_track > count) count = (uint)max_track;
                return count;
            }
        }

        public uint CurrentDisc
        {
            get
            {
                if (Info == null) return 0;
                uint disc = Info?.Mp3Fields.Disc ?? 1;
                if (disc == 0) disc = 1;
                return disc;
            }
        }

        public uint DiscCount
        {
            get
            {
                if (Info == null) return 0;
                uint discs = Info?.Mp3Fields.DiscCount ?? 1;
                if (discs == 0) discs = 1;
                return discs;
            }
        }


        public string DiscLength
        {
            get
            {
                if (AlbumTracks == null || !AlbumTracks.Any())
                    return string.Empty;

                double len = 0;
                foreach (var track in AlbumTracks)
                    len += track.DurationValue;

                return Utilities.LibraryUtils.GetDurationString((int)len);
            }
        }

        public void SetInfo(FileViewInfo info)
        {
            Info = info;
            AlbumImage.Source = ImageConnector.GetImageFromFile(Info.Path);

            if (AlbumImage.Source == null)
            {
                AlbumImage.Source = StandardImage.DefaultAlbumImage;
                AlbumImage.Opacity = 0.4;
            }
            else
            {
                AlbumImage.Opacity = 1;
            }

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
            var filteredTracks = new List<FileViewInfo>();

            foreach (var t in tracks)
            {
                if (filteredTracks.Any(f => f.Title == t.Title && f.Number == t.Number && f.Mp3Fields.DurationValue == t.Mp3Fields.DurationValue))
                    continue;

                else filteredTracks.Add(t);
            }

            AlbumTracks = from track in filteredTracks
                          select new SimpleTrackItem
                          {
                              Length = track.Mp3Fields.Duration,
                              Name = track.Title,
                              Track = (int)track.Mp3Fields.Track,
                              DurationValue = (double)track.Mp3Fields.DurationValue
                            };

            Raise(nameof(DiscLength));
            Raise(nameof(TrackCount));
            Raise(nameof(CurrentDisc));
            Raise(nameof(DiscCount));

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

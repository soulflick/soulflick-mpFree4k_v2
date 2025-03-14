using Classes;
using Models;
using MpFree4k.Classes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace Dialogs
{

    public partial class PlaylistCover : Window, INotifyPropertyChanged
    {
        public PlaylistCover(IEnumerable<PlaylistInfo> collection)
        {
            foreach (var item in collection)
                Collection.Add(new PlaylistInfo()
                {
                    TrackNumber = item.TrackNumber,
                    Duration = item.Duration,
                    DurationValue = item.DurationValue,
                    Artists = item.Artists,
                    Title = item.Title,
                    Album = item.Album,
                    Year = item.Year,
                    Path = item.Path,
                });

            //foreach (var item in Collection)
            //{
            //    var fileItem = Layers.Library.Instance.Current.Files.FirstOrDefault(f => f.Path == item.Path);
            //    if (fileItem != null) item.Image = fileItem.TrackImage;
            //}

            InitializeComponent();
            DataContext = this;
            SetInfos();
        }

        public List<PlaylistInfo> Collection { get; set; } = new List<PlaylistInfo>();
        public FileViewInfo Info { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public uint CompilationCount => (uint)Collection.Count();

        private string _compilationName = "New Compilation";
        public string CompilationName
        {
            get => _compilationName;
            set
            {
                _compilationName = value;
                Raise(nameof(CompilationName));
            }
        }

        private uint _compilationYear = 2025;
        public uint CompilationYear
        {
            get => _compilationYear;
            set
            {
                _compilationYear = value;
                Raise(nameof(CompilationYear));
            }
        }

        public string CompilationLength
        {
            get
            {
                if (Collection == null || !Collection.Any())
                    return string.Empty;

                double len = 0;
                foreach (var track in Collection)
                    len += track.DurationValue;

                return Utilities.LibraryUtils.GetDurationString((int)len);
            }
        }


        public void SetInfos()
        {
            AlbumImage.Source = StandardImage.DefaultAlbumImage;
            AlbumImage.Opacity = 0.4;
            Raise(nameof(CompilationLength));

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

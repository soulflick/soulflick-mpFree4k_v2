using Mpfree4k.Enums;
using MpFree4k;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModels;

namespace Controls
{
    public partial class TableView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public TableView() => InitializeComponent();

        ArtistViewType _artistViewType = ArtistViewType.List;
        public ArtistViewType ArtistViewType
        {
            get => _artistViewType;
            set { _artistViewType = value; Raise(nameof(ArtistViewType)); }
        }

        AlbumViewType _albumViewType = AlbumViewType.List;
        public AlbumViewType AlbumViewType
        {
            get => _albumViewType;
            set { _albumViewType = value; Raise(nameof(AlbumViewType)); }
        }

        TrackViewType _trackViewType = TrackViewType.List;
        public TrackViewType TrackViewType
        {
            get => _trackViewType;
            set { _trackViewType = value; Raise(nameof(TrackViewType)); }
        }

        ArtistOrderType _artistOrderType = ArtistOrderType.Artist;
        public ArtistOrderType ArtistOrderType
        {
            get => _artistOrderType;
            set { _artistOrderType = value; Raise(nameof(ArtistOrderType)); }
        }

        AlbumOrderType _albumOrderType = AlbumOrderType.Album;
        public AlbumOrderType AlbumOrderType
        {
            get => _albumOrderType;
            set { _albumOrderType = value; Raise(nameof(AlbumOrderType)); }
        }

        TrackOrderType _trackOrderType = TrackOrderType.Standard;
        public TrackOrderType TrackOrderType
        {
            get => _trackOrderType;
            set { _trackOrderType = value; Raise(nameof(TrackOrderType)); }
        }

        private void ArtistViewType_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab.Tag is ArtistViewType)
            {
                ArtistViewType type = (ArtistViewType)(sender as Label).Tag;
                ArtistViewType = type;
                ArtistView.SetViewType(type);
            }
        }

        private void AlbumViewType_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab.Tag is AlbumViewType)
            {
                AlbumViewType type = (AlbumViewType)(sender as Label).Tag;
                AlbumViewType = type;
                AlbumView.SetViewType(type);
            }
        }

        private void TrackViewType_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab.Tag is TrackViewType)
            {
                TrackViewType type = (TrackViewType)(sender as Label).Tag;
                TrackViewType = type;
                TrackView.SetViewType(type);
            }
        }

        private void ArtistOrderType_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab.Tag is ArtistOrderType)
            {
                ArtistOrderType type = (ArtistOrderType)(sender as Label).Tag;
                ArtistOrderType = type;
                (ArtistView.DataContext as ArtistsViewModel).OrderBy(type);
            }
        }


        private void AlbumOrderType_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab.Tag is AlbumOrderType)
            {
                AlbumOrderType type = (AlbumOrderType)(sender as Label).Tag;
                AlbumOrderType = type;
                (AlbumView.DataContext as AlbumsViewModel).OrderBy(type);
            }
        }

        private void TrackOrderType_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab.Tag is TrackOrderType)
            {
                TrackOrderType type = (TrackOrderType)(sender as Label).Tag;
                TrackOrderType = type;
                (TrackView.DataContext as TracksViewModel).OrderBy(type);
            }
        }

        private void _This_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.ViewMode == ViewMode.Table)
                MainWindow.Instance.Library.Current.QueryMe(ViewMode.Details);
        }
    }
}

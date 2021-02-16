using MpFree4k.Enums;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModels;

namespace MpFree4k.Controls
{
    public partial class TableView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public TableView()
        {
            InitializeComponent();
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


        private ArtistViewType _artistViewType = ArtistViewType.List;
        public AlbumViewType _albumViewType = AlbumViewType.List;
        public TrackViewType _trackViewType = TrackViewType.List;


        public ArtistViewType ArtistViewType { get { return _artistViewType; } set { _artistViewType = value; OnPropertyChanged("ArtistViewType"); } }
        public AlbumViewType AlbumViewType { get { return _albumViewType; } set { _albumViewType = value; OnPropertyChanged("AlbumViewType"); } }
        public TrackViewType TrackViewType { get { return _trackViewType; } set { _trackViewType = value; OnPropertyChanged("TrackViewType"); } }

        private ArtistOrderType _artistOrderType = ArtistOrderType.Artist;
        public AlbumOrderType _albumOrderType = AlbumOrderType.Album;
        public TrackOrderType _trackOrderType = TrackOrderType.Standard;

        public ArtistOrderType ArtistOrderType { get { return _artistOrderType; } set { _artistOrderType = value; OnPropertyChanged("ArtistOrderType"); } }
        public AlbumOrderType AlbumOrderType { get { return _albumOrderType; } set { _albumOrderType = value; OnPropertyChanged("AlbumOrderType"); } }
        public TrackOrderType TrackOrderType { get { return _trackOrderType; } set { _trackOrderType = value; OnPropertyChanged("TrackOrderType"); } }

        private void _This_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.ViewMode == ViewMode.Table)
                MainWindow.Instance.Library.Current.QueryMe(ViewMode.Details);
        }
    }
}

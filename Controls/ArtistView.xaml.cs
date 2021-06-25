using Classes;
using Configuration;
using Mpfree4k.Enums;
using Models;
using MpFree4k;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ViewModels;


namespace Controls
{
    public partial class ArtistView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };
        private List<PlaylistInfo> dragItems = new List<PlaylistInfo>();
        private List<SimpleAlbumItem> selected_simple_albums = new List<SimpleAlbumItem>();
        bool mousedown = false;
        bool prevent_side_selection = false;
        Point mousepos = new Point(0, 0);

        public void Raise(string info)  => PropertyChanged(this, new PropertyChangedEventArgs(info));

        public ArtistView()
        {
            DataContext = new ArtistsViewModel();
            (DataContext as ArtistsViewModel).PropertyChanged += ArtistView_PropertyChanged;
            Loaded += ArtistView_Loaded;
            InitializeComponent();
            ListWidth = calcListWidth();
        }

        private void ArtistView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Artists")
                ListWidth = calcListWidth();
            if (e.PropertyName == "IndexArtists")
            {
            }
        }

        private void ArtistView_Loaded(object sender, RoutedEventArgs e) => ListWidth = calcListWidth();

        public void SetMediaLibrary(Layers.MediaLibrary lib) => (this.DataContext as ArtistsViewModel).MediaLibrary = lib;

        private void ListArtists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] artists = new string[0];

            if ((sender as ListView).SelectedItems != null &&
                (sender as ListView).SelectedItems.Count > 0)
            {
                artists = new string[(sender as ListView).SelectedItems.Count];
                for (int i = 0; i < (sender as ListView).SelectedItems.Count; i++)
                    artists[i] = ((sender as ListView).SelectedItems[i] as ArtistItem).Artists;
            }

            (this.DataContext as ArtistsViewModel).MediaLibrary.Filter(MediaLevel.Artists, artists);


        }

        double calcListWidth(double add = -4)
        {
            ListView view = ListArtists;
            double wid = view.ActualWidth;

            ScrollViewer scrollview = Utilities.VisualTreeHelper.GetVisualChild<ScrollViewer, ListView>(view);
            if (scrollview != null)
            {
                Visibility verticalVisibility = scrollview.ComputedVerticalScrollBarVisibility;
                if (verticalVisibility != Visibility.Collapsed)
                {
                    double substract = SystemParameters.VerticalScrollBarWidth;
                    wid -= substract;
                }
            }

            return wid + add;
        }

        private double _listWidth = -1;
        public double ListWidth
        {
            set
            {
                _listWidth = value;
                Raise(nameof(ListWidth));
            }
            get => _listWidth;
        }



        private ArtistViewType _artistViewType = Mpfree4k.Enums.ArtistViewType.List;
        public ArtistViewType ArtistViewType
        {
            get => _artistViewType;
            set
            {
                _artistViewType = value;
                Raise(nameof(ArtistViewType));
            }
        }

        public void SetViewType(ArtistViewType type)
        {
            ArtistViewType = type;
            UserConfig.ArtistViewType = type;
        }

        private void ListArtists_SizeChanged(object sender, SizeChangedEventArgs e) => ListWidth = calcListWidth();

        private void ListArtists_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mousepos.X != 0 && mousepos.Y != 0 && mousedown && e.LeftButton == MouseButtonState.Pressed && e.OriginalSource is TextBlock)
            {
                Point curpos = e.MouseDevice.GetPosition(this);
                double x = Math.Abs(mousepos.X - curpos.X);
                double y = Math.Abs(mousepos.Y - curpos.Y);
                if (x < Config.drag_pixel || y < Config.drag_pixel)
                    return;

                DragDrop.DoDragDrop(ListArtists, new DataObject("MediaLibraryItemData", dragItems), DragDropEffects.Move);
            }
        }

        private void ListArtists_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            mousedown = true;
            mousepos = e.MouseDevice.GetPosition(this);
            dragItems.Clear();
            foreach (FileViewInfo info in TracksViewModel.Instance.Tracks.Where(x => x.IsVisible).ToList())
            {
                PlaylistInfo plitm = new PlaylistInfo();
                PlaylistHelpers.CreateFromMediaItem(plitm, info);
                dragItems.Add(plitm);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (prevent_side_selection)
                return;

            prevent_side_selection = true;
            ListBox box = (System.Windows.Controls.ListBox)sender;

            box.SelectionChanged -= ListBox_SelectionChanged;
            ListArtists.SelectionChanged -= ListArtists_SelectionChanged;

            lock (selected_simple_albums)
            {
                if (!MainWindow.ctrl_down)
                {
                    if (selected_simple_albums.Count > 0)
                        foreach (SimpleAlbumItem al in selected_simple_albums)
                            al.IsSelected = false;

                    selected_simple_albums.Clear();
                }

                selected_simple_albums.RemoveAll(x => !x.IsSelected);

                if (box.SelectedItems != null && box.SelectedItems.Count > 0)
                {
                    foreach (SimpleAlbumItem ai in box.SelectedItems)
                    {
                        selected_simple_albums.Add(ai);
                    }
                }

                if (selected_simple_albums.Count > 0)
                {
                    (this.DataContext as ArtistsViewModel).MediaLibrary.Set(selected_simple_albums);

                    List<AlbumItem> albums = new List<AlbumItem>();
                    foreach (SimpleAlbumItem _ai in selected_simple_albums)
                    {
                        AlbumItem aai = new AlbumItem()
                        {
                            Album = _ai.AlbumLabel,
                            Artist = _ai.Artist,
                            AllArtist = new List<string>() { _ai.Artist },
                            Genre = _ai.Genre,
                            TrackCount = _ai.TrackCount,
                            Year = _ai.Year

                        };
                        albums.Add(aai);
                    }
                (this.DataContext as ArtistsViewModel).MediaLibrary.Filter(MediaLevel.Albums, albums);
                }
                else
                {
                    (this.DataContext as ArtistsViewModel).MediaLibrary.Filter(MediaLevel.Artists);
                }
            }

            prevent_side_selection = false;
            box.SelectionChanged += ListBox_SelectionChanged;
            ListArtists.SelectionChanged += ListArtists_SelectionChanged;

            e.Handled = true;


        }

        private void ListArtists_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (MainWindow.ctrl_down && e.Key == Key.A)
            {
                ArtistsViewModel VM = this.DataContext as ArtistsViewModel;
                VM.Artists.Where(a => a.IsVisible).ToList().ForEach(x => x.IsSelected = true);
                VM.MediaLibrary.Refresh(MediaLevel.All);
                e.Handled = true;
            }
        }

        private void ListArtists_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            mousedown = false;
            mousepos = new Point(0, 0);
        }
    }
}

using Classes;
using ViewModels;
using Mpfree4k.Enums;
using Layers;
using Models;
using Dialogs;
using Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Configuration;
using MpFree4k;

namespace Controls
{
    public partial class AlbumView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private AlbumsViewModel viewModel;
        public void SetMediaLibrary(MediaLibrary lib) => viewModel.MediaLibrary = lib;

        private List<PlaylistItem> dragItems = new List<PlaylistItem>();
        bool mousedown = false;
        Point mousepos = new Point(0, 0);
        
        public AlbumView()
        {
            DataContext = viewModel = new AlbumsViewModel();
            viewModel.PropertyChanged += AlbumView_PropertyChanged;
           
            InitializeComponent();

            Loaded += AlbumView_Loaded;
        }

        private void AlbumView_Loaded(object sender, RoutedEventArgs e) => viewModel?.UpdateAmount();

        private void AlbumView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Albums")
            {
                ListWidth = calcListWidth();
            }
            else if (e.PropertyName == "IndexAlbums")
            {
                AlbumItem firstSelectedItem = (_This.DataContext as AlbumsViewModel).Albums.FirstOrDefault(a => a.IsSelected);
                if (firstSelectedItem != null)
                {
                    ListAlbums.ScrollIntoView(firstSelectedItem);
                }
            }
        }

        private void ListAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<AlbumItem> albums = new List<AlbumItem>();
            if ((sender as ListView).SelectedItems != null &&
               (sender as ListView).SelectedItems.Count > 0)
            {
                for (int i = 0; i < (sender as ListView).SelectedItems.Count; i++)
                    albums.Add((sender as ListView).SelectedItems[i] as AlbumItem); ;
            }

            viewModel.MediaLibrary.Filter(MediaLevel.Albums, albums);

        }

        private void ListAlbums_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mousepos.X != 0 && mousepos.Y != 0 && mousedown && e.LeftButton == MouseButtonState.Pressed && e.OriginalSource is TextBlock)
            {
                Point curpos = e.MouseDevice.GetPosition(this);
                double x = Math.Abs(mousepos.X - curpos.X);
                double y = Math.Abs(mousepos.Y - curpos.Y);
                if (x < Config.drag_pixel || y < Config.drag_pixel)
                    return;

                DragDrop.DoDragDrop(ListAlbums, new DataObject("MediaLibraryItemData", dragItems), DragDropEffects.Move);
            }
        }

        private void ListAlbums_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            mousedown = true;
            mousepos = e.MouseDevice.GetPosition(this);

            dragItems.Clear();

            foreach (FileViewInfo info in TracksViewModel.Instance.Tracks.Where(x => x.IsVisible).ToList())
            {
                PlaylistItem plitm = new PlaylistItem();
                PlaylistHelpers.CreateFromMediaItem(plitm, info);
                dragItems.Add(plitm);
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        double calcListWidth(double add = -4)
        {
            ListView view = ListAlbums;
            double wid = view.ActualWidth;

            ScrollViewer scrollview = FindVisualChild<ScrollViewer>(view);
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
            get => _listWidth;
            set 
            {
                _listWidth = value;
                Raise(nameof(ListWidth));
            }
        }

        private void ListAlbums_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListWidth = calcListWidth();
        }

        private AlbumViewType _albumViewType = Mpfree4k.Enums.AlbumViewType.List;
        public AlbumViewType AlbumViewType
        {
            get => _albumViewType;
            set
            {
                _albumViewType = value;
                Raise(nameof(AlbumViewType));
            }
        }

        public void SetViewType(AlbumViewType type)
        {
            AlbumViewType = type;
            UserConfig.AlbumViewType = type;
        }

        private void ListAlbums_KeyDown(object sender, KeyEventArgs e)
        {
            if (MainWindow.ctrl_down && e.Key == Key.A)
            {
                AlbumsViewModel VM = this.DataContext as AlbumsViewModel;
                VM.Albums.Where(a => a.IsVisible).ToList().ForEach(x => x.IsSelected = true);
                VM.MediaLibrary.Refresh(MediaLevel.All);
                e.Handled = true;
            }
        }

        private void ListAlbums_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            mousedown = false;
            mousepos = new Point(0, 0);
        }

        private void ListViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AlbumsViewModel VM = this.DataContext as AlbumsViewModel;
            AlbumItem itm = (sender as ListViewItem).DataContext as AlbumItem;
            string[] tracks = VM.MediaLibrary.Files.Where(f => f.Mp3Fields.Year == itm.Year && f.Mp3Fields.Album == itm.Album).Select(y => y.Path).ToArray();
            Library.Instance.connector.SetAlbum(itm.Album, itm.Year.ToString(), tracks);
        }

        private void ListAlbums_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double wid = calcListWidth();
            if (ListWidth != wid)
                ListWidth = wid;
        }

        private void mnuCtxPlay_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is AlbumItem album)
            {
                PlaylistViewModel.Play(LibraryUtils.GetTracks(album));
            }
        }

        private void mnuCtxInsert_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is AlbumItem album)
            {
                PlaylistViewModel.Insert(LibraryUtils.GetTracks(album));
            }
        }

        private void mnuCtxAdd_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem)?.DataContext is AlbumItem album)
            {
                PlaylistViewModel.Add(LibraryUtils.GetTracks(album));
            }
        }

        private void mnuCtxEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ListAlbums.SelectedItem is AlbumItem album)
            {
                FileViewInfo[] infos = LibraryUtils.GetInfoItems(album);

                TracksEditor editor = new TracksEditor(infos);
                editor.ShowDialog();
            }
        }
    }
}

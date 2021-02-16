using Classes;
using MpFree4k.Classes;
using MpFree4k.Dialogs;
using MpFree4k.Enums;
using MpFree4k.Utilies;
using MpFree4k.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpFree4k.Controls
{
    public enum SelectedControl
    {
        None, 
        Albums,
        Tracks
    }

    public partial class Favourites : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };
        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        FavouritesViewModel VM = null;

        public Thickness TableMargin { get; set; } = new Thickness(30);

        public Favourites()
        {
            VM = new FavouritesViewModel();

            this.DataContext = VM;

            this.Loaded += Favourites_Loaded;

            InitializeComponent();

        }

        bool loaded = false;
        private void Favourites_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {
                TableMargin = new Thickness(3);
                OnPropertyChanged("TableMargin");
            }

            if (MainWindow.Instance.MainViews.SelectedIndex == (int)TabOrder.Favourites)
                VM.Load();
        }

        public void Reload()
        {
            VM.Load();
        }

        public void UpdateMargín(FontSize size)
        {
            TableMargin = new Thickness(2 * (int)size);
            OnPropertyChanged("TableMargin");
        }

        List<SimpleAlbumItem> dragItems_albums = new List<SimpleAlbumItem>();
        List<FileViewInfo> dragItems_tracks = new List<FileViewInfo>();
        private void TrackTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dragItems_tracks.Clear();

            foreach (FileViewInfo infoItm in TrackTable.SelectedItems)
            {
                if (!infoItm.IsVisible || infoItm == null)
                {
                    SelectedControl = SelectedControl.None;
                    continue;
                }

                SelectedControl = SelectedControl.Tracks;
                dragItems_tracks.Add(infoItm);
            }
        }

        private bool mousedown_tracks = false;
        private bool mousedown_albums = false;
        private void TrackTable_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            mousedown_tracks = true;
            mousepos = e.MouseDevice.GetPosition(this);
            var item = ItemsControl.ContainerFromElement(TrackTable, e.OriginalSource as DependencyObject) as DataGridRow;

            if (item != null)
            {
                if (!MainWindow.ctrl_down)
                {
                    if (item.IsSelected)
                        e.Handled = true;
                    else
                    {
                        TrackTable.SelectedItems.Clear();
                        item.IsSelected = true;
                    }
                }
                else
                {
                    if (item.IsSelected)
                    {
                        if (TrackTable.SelectedItems.Contains(item))
                            TrackTable.SelectedItems.Remove(item);
                        else
                            TrackTable.SelectedItems.Add(item);
                    }
                    //item.IsSelected = !item.IsSelected;
                }

            }
        }

        private void TrackTable_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mousepos.X != 0 && mousepos.Y != 0 && mousedown_tracks && e.LeftButton == MouseButtonState.Pressed && e.OriginalSource is TextBlock)
            {
                Point curpos = e.MouseDevice.GetPosition(this);
                double x = Math.Abs(mousepos.X - curpos.X);
                double y = Math.Abs(mousepos.Y - curpos.Y);
                if (x < Config.drag_pixel || y < Config.drag_pixel)
                    return;

                DragDrop.DoDragDrop(TrackTable, new DataObject("MediaLibraryFileInfos", dragItems_tracks), DragDropEffects.Move);
            }
        }

        private void TrackTable_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileViewInfo f_Sel = TrackTable.SelectedItem as FileViewInfo;
            PlaylistItem p_i = new PlaylistItem();
            PlaylistHelpers.CreateFromMediaItem(p_i, f_Sel);
            PlaylistViewModel VM = (MainWindow.Instance).Playlist.DataContext as PlaylistViewModel;

            int playpos = VM.CurrentPlayPosition;

            if (VM.Tracks.Count > 0)
            {
                if (VM.Tracks.Count <= playpos)
                    playpos = VM.Tracks.Count;
                else
                    playpos++;
            }
            else
                playpos = 0;

            VM.Add(new List<PlaylistItem>() { p_i }, playpos);
            PlaylistItem cloned = VM.Tracks[playpos];
            VM.enumerate(playpos);
            VM.CurrentPlayPosition = cloned._position - 1;
            (MainWindow.Instance.Playlist.DataContext as PlaylistViewModel).Invoke(PlayState.Play);

        }

        Point mousepos = new Point(0, 0);
        private void TrackTable_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            mousedown_tracks = false;
            mousepos = new Point(0, 0);
        }

        private void ListAlbums_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mousepos.X != 0 && mousepos.Y != 0 && mousedown_albums && e.LeftButton == MouseButtonState.Pressed && e.OriginalSource is TextBlock)
            {
                if (!dragItems_albums.Any())
                    return;

                Point curpos = e.MouseDevice.GetPosition(this);
                double x = Math.Abs(mousepos.X - curpos.X);
                double y = Math.Abs(mousepos.Y - curpos.Y);
                if (x < Config.drag_pixel || y < Config.drag_pixel)
                    return;

                DragDrop.DoDragDrop(ListAlbums, new DataObject("AlbumItemData", dragItems_albums), DragDropEffects.Move);
            }
        }

        private void ListAlbums_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            mousedown_albums = true;
            mousepos = e.MouseDevice.GetPosition(this);
        }

        private void ListAlbums_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            mousedown_albums = false;
            mousepos = new Point(0, 0);
        }

        private SelectedControl SelectedControl = SelectedControl.None;
        private void ListAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            dragItems_albums.Clear();

            SimpleAlbumItem a = (sender as ListView).SelectedItem as SimpleAlbumItem;
            if (a == null || a.Tracks == null || !a.Tracks.Any())
            {
                SelectedControl = SelectedControl.None;
                return;
            }

            SelectedControl = SelectedControl.Albums;
            dragItems_albums.Add(a);

        }

        private void ListAlbums_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete)
                return;

            List<SimpleAlbumItem> ai = new List<SimpleAlbumItem>();
            foreach (var i in (sender as ListView).SelectedItems)
                ai.Add(i as SimpleAlbumItem);

            FavouritesViewModel VM = (this.DataContext as FavouritesViewModel);
            VM.Remove(ai);
            VM.LoadAlbums();
        }

        private void mnuAlbumCtxPlay_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).DataContext is SimpleAlbumItem album)
            {
                PlaylistViewModel.Play(LibraryUtils.GetTracks(album));
            }
        }

        private void mnuAlbumCtxInsert_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).DataContext is SimpleAlbumItem album)
            {
                PlaylistViewModel.Play(LibraryUtils.GetTracks(album));
            }
        }

        private void mnuAlbumCtxAdd_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).DataContext is SimpleAlbumItem album)
            {
                PlaylistViewModel.Play(LibraryUtils.GetTracks(album));
            }
        }

        private void mnuAlbumCtxEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).DataContext is SimpleAlbumItem album)
            {
                FileViewInfo[] infos = LibraryUtils.GetInfoItems(album);

                TracksEditor editor = new TracksEditor(infos);
                editor.ShowDialog();
            }
        }

        private void mnuTrackCtxPlay_Click(object sender, RoutedEventArgs e)
        {
            if (TrackTable.SelectedItem == null)
                return;

            List<PlaylistItem> items = new List<PlaylistItem>();

            foreach (var f_Sel in TrackTable.SelectedItems)
            {
                PlaylistItem p_i = new PlaylistItem();
                PlaylistHelpers.CreateFromMediaItem(p_i, (FileViewInfo)f_Sel);
                items.Add(p_i);
            }

            PlaylistViewModel.Play(items.ToArray());
        }

        public PlaylistItem[] GetSelected()
        {
            switch (SelectedControl)
            {
                case SelectedControl.None: return null;
                case SelectedControl.Albums:
                    {
                        if (dragItems_albums == null || !dragItems_albums.Any())
                            return null;

                        return LibraryUtils.GetItems(dragItems_albums[0].Tracks.ToArray()).ToArray();
                    }
                case SelectedControl.Tracks:
                    {
                        if (dragItems_tracks.Count == 0)
                            return null;

                        List<PlaylistItem> items = new List<PlaylistItem>();
                        foreach (var it in dragItems_tracks)
                        {
                            PlaylistItem pli = PlaylistHelpers.CreateFromMediaItem(it);
                            items.Add(pli);
                        }

                        return items.ToArray();
                    }
                default: return null;

            }
        }
        
        private void mnuTrackCtxInsert_Click(object sender, RoutedEventArgs e)
        {
            if (TrackTable.SelectedItem == null)
                return;

            PlaylistItem[] items = TrackTable.SelectedItems.Cast<PlaylistItem>().ToArray();
            PlaylistViewModel.Insert(items);
        }

        private void mnuTrackCtxAdd_Click(object sender, RoutedEventArgs e)
        {
            if (TrackTable.SelectedItem == null)
                return;

            PlaylistItem[] items = TrackTable.SelectedItems.Cast<PlaylistItem>().ToArray();
            PlaylistViewModel.Insert(items);
        }

        private void mnuTrackCtxEdit_Click(object sender, RoutedEventArgs e)
        {
            if (TrackTable.SelectedItems == null ||
                TrackTable.SelectedItems.Count == 0)
                return;

            FileViewInfo[] infos = TrackTable.SelectedItems.Cast<FileViewInfo>().ToArray();
            TracksEditor editor = new TracksEditor(infos);
            editor.ShowDialog();
        }
    }
}

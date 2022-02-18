using Classes;
using Models;
using Dialogs;
using Utilities;
using ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Mpfree4k.Enums;
using Configuration;
using MpFree4k;

namespace Controls
{
    public partial class TrackTableView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        public Thickness TableMargin { get; set; } = new Thickness(30);

        private bool loaded = false;
        private bool mousedown = false;
        private Point mousepos = new Point(0, 0);
        private List<FileViewInfo> dragItems = new List<FileViewInfo>();

        public TrackTableView()
        {
            DataContext = new TrackTableViewModel();
            Loaded += TrackTableView_Loaded;
            InitializeComponent();
        }

        private TrackTableViewModel ViewModel => DataContext as TrackTableViewModel;

        public void SetMediaLibrary(Layers.MediaLibrary lib) => ViewModel.MediaLibrary = lib;

        public void UpdateMargín(FontSize size)
        {
            TableMargin = new Thickness(2 * (int)size);
            Raise(nameof(TableMargin));
        }

        public List<PlaylistInfo> GetSelected()
        {
            if (TrackTable.SelectedItem == null)
                return null;

            List<PlaylistInfo> plItems = new List<PlaylistInfo>();
            List<FileViewInfo> items = TrackTable.SelectedItems.Cast<FileViewInfo>().ToList();
            items.ForEach(item => plItems.Add(PlaylistHelpers.CreateFromFileViewInfo(item)));

            return plItems;
        }

        private FileViewInfo GetFirstSelected()
        {
            if (TrackTable.SelectedItem == null)
                return null;

            return TrackTable.SelectedItems[0] as FileViewInfo;
        }

        private void playSelected()
        {
            if (TrackTable.SelectedItem == null)
                return;

            List<PlaylistInfo> items = new List<PlaylistInfo>();

            foreach (var f_Sel in TrackTable.SelectedItems)
            {
                PlaylistInfo p_i = new PlaylistInfo();
                PlaylistHelpers.CreateFromMediaItem(p_i, (FileViewInfo)f_Sel);
                items.Add(p_i);
            }

            PlaylistViewModel.Play(items.ToArray());
        }

        private void _This_Loaded(object sender, RoutedEventArgs e) => MainWindow.Instance.Library.Current.QueryMe(ViewMode.Table);

        private void TrackTableView_Loaded(object sender, RoutedEventArgs e)
        {
            if (loaded && !Config.MediaHasChanged)
                return;

            TableMargin = new Thickness(3);

            loaded = true;
            Config.MediaHasChanged = false;
        }

        private void TrackTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dragItems.Clear();

            foreach (FileViewInfo infoItm in TrackTable.SelectedItems)
            {
                if (!infoItm.IsVisible)
                    continue;

                dragItems.Add(infoItm);
            }
        }

        private void TrackTable_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            mousedown = true;
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
                }
            }
        }

        private void TrackTable_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mousepos.X != 0 && mousepos.Y != 0 && mousedown && e.LeftButton == MouseButtonState.Pressed && e.OriginalSource is TextBlock)
            {
                Point curpos = e.MouseDevice.GetPosition(this);
                double x = Math.Abs(mousepos.X - curpos.X);
                double y = Math.Abs(mousepos.Y - curpos.Y);
                if (x < Config.drag_pixel || y < Config.drag_pixel)
                    return;

                DragDrop.DoDragDrop(TrackTable, new DataObject("MediaLibraryFileInfos", dragItems), DragDropEffects.Move);
            }
        }

        private void TrackTable_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) => playSelected();
        
        private void TrackTable_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            mousedown = false;
            mousepos = new Point(0,0);
        }

        private void mnuCtxEdit_Click(object sender, RoutedEventArgs e)
        {
            if (TrackTable.SelectedItems == null ||
                TrackTable.SelectedItems.Count == 0)
                return;
           
            FileViewInfo[] infos = TrackTable.SelectedItems.Cast<FileViewInfo>().ToArray();
            TracksEditor editor = new TracksEditor(infos);
            editor.ShowDialog();
        }

        private void mnuCtxPlay_Click(object sender, RoutedEventArgs e) => playSelected();

        private void mnuCtxAdd_Click(object sender, RoutedEventArgs e)
        {
            var items = GetSelected();
            if (items == null)
                return;

            PlaylistViewModel.Add(items.ToArray());
        }

        private void mnuCtxInsert_Click(object sender, RoutedEventArgs e)
        {
            var items = GetSelected();
            if (items == null)
                return;

            PlaylistViewModel.Insert(items.ToArray());
        }

        private void mnuCtxAddAlbum_Click(object sender, RoutedEventArgs e)
        {
            var item = GetFirstSelected();
            if (item == null)
                return;

            var vm = ViewModel;
            var albumItems = LibraryUtils.GetAlbumItems(item, vm.LibraryTracks.ToList());
            PlaylistViewModel.Instance.Add(albumItems);
        }

        private void mnuCtxInsertAlbum_Click(object sender, RoutedEventArgs e)
        {
            var item = GetFirstSelected();
            if (item == null)
                return;

            var vm = ViewModel;
            var albumItems = LibraryUtils.GetAlbumItems(item, vm.LibraryTracks.ToList());
            PlaylistViewModel.Insert(albumItems);
        }

        private void mnuCtxPlayAlbum_Click(object sender, RoutedEventArgs e)
        {
            var item = GetFirstSelected();
            if (item == null)
                return;

            var vm = ViewModel;
            var albumItems = LibraryUtils.GetAlbumItems(item, vm.LibraryTracks.ToList());
            PlaylistViewModel.Play(albumItems);
        }

        private void mnuCtxPlayAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = ViewModel;
            var uniques = vm.Tracks.DistinctBy(x => x.Title).ToArray();
            PlaylistViewModel.Play(uniques);
        }

        private void flagTracks(FlagType fType)
        {
            if (TrackTable.SelectedItems == null || TrackTable.SelectedItems.Count == 0) return;
            foreach (var track in TrackTable.SelectedItems.Cast<FileViewInfo>()) track.SetFlag(fType);
        }

        private void mnuCtxPlayThisArtist_Click(object sender, RoutedEventArgs e)
        {
            if (TrackTable.SelectedItems == null ||
                TrackTable.SelectedItems.Count == 0)
                return;

            List<string> artists = new List<string>();
            foreach (var track in TrackTable.SelectedItems.Cast<FileViewInfo>()) { artists.Add(track.Mp3Fields.AlbumArtists); };
            artists = artists.Distinct().ToList();
            var selection = ViewModel.LibraryTracks.Where(track => artists.Contains(track.Mp3Fields.AlbumArtists)).Distinct();
            selection = selection.DistinctBy(y => y.Mp3Fields.FileName);

            PlaylistViewModel.Play(selection.ToArray());
        }

        private void mnuCtxAddThisArtist_Click(object sender, RoutedEventArgs e)
        {
            if (TrackTable.SelectedItems == null ||
                TrackTable.SelectedItems.Count == 0)
                return;

            List<string> artists = new List<string>();
            foreach (var track in TrackTable.SelectedItems.Cast<FileViewInfo>()) { artists.Add(track.Mp3Fields.AlbumArtists); };
            artists = artists.Distinct().ToList();
            var selection = ViewModel.LibraryTracks.Where(track => artists.Contains(track.Mp3Fields.AlbumArtists)).Distinct();
            selection = selection.DistinctBy(y => y.Mp3Fields.FileName);

            PlaylistViewModel.AddTracks(selection.ToArray());
        }

        private void mnuCtxFlagOK_Click(object sender, RoutedEventArgs e)
        {
            flagTracks(FlagType.OK);
        }

        private void mnuCtxFlagTagged_Click(object sender, RoutedEventArgs e)
        {
            flagTracks(FlagType.Tagged);
        }

        private void mnuCtxFlagHidden_Click(object sender, RoutedEventArgs e)
        {
            flagTracks(FlagType.Hidden);
        }

        private void mnuCtxFlagEasy_Click(object sender, RoutedEventArgs e)
        {
            flagTracks(FlagType.Easy);
        }

        private void mnuCtxFlagNew_Click(object sender, RoutedEventArgs e)
        {
            flagTracks(FlagType.New);
        }

        private void mnuCtxFlagFailures_Click(object sender, RoutedEventArgs e)
        {
            flagTracks(FlagType.Failures);
        }

        private void mnuCtxFlagUnknown_Click(object sender, RoutedEventArgs e)
        {
            flagTracks(FlagType.Unknown);
        }

        private void mnuCtxInsertThisArtist_Click(object sender, RoutedEventArgs e)
        {
            if (TrackTable.SelectedItems == null ||
                TrackTable.SelectedItems.Count == 0)
                return;

            List<string> artists = new List<string>();
            foreach (var track in TrackTable.SelectedItems.Cast<FileViewInfo>()) { artists.Add(track.Mp3Fields.AlbumArtists); };
            artists = artists.Distinct().ToList();
            var selection = ViewModel.LibraryTracks.Where(track => artists.Contains(track.Mp3Fields.AlbumArtists)).Distinct();
            selection = selection.DistinctBy(y => y.Mp3Fields.FileName);

            PlaylistViewModel.Insert(selection.ToArray());
        }
    }
}

using Classes;
using MpFree4k.Classes;
using MpFree4k.Dialogs;
using MpFree4k.Enums;
using MpFree4k.Utilities;
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
    public partial class TrackTableView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };
        public void OnPropertyChanged(String info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        public Thickness TableMargin { get; set; } = new Thickness(30);

        private bool loaded = false;
        private bool mousedown = false;
        private Point mousepos = new Point(0, 0);
        private List<FileViewInfo> dragItems = new List<FileViewInfo>();

        public TrackTableView()
        {
            this.DataContext = new TrackTableViewModel();
            Loaded += TrackTableView_Loaded;
            InitializeComponent();
        }

        public void SetMediaLibrary(Layers.MediaLibrary lib) => (this.DataContext as TrackTableViewModel).MediaLibrary = lib;

        public void UpdateMargín(FontSize size)
        {
            TableMargin = new Thickness(2 * (int)size);
            OnPropertyChanged("TableMargin");
        }

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
        
        private void playSelected()
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

        private void _This_Loaded(object sender, RoutedEventArgs e) => MainWindow.Instance.Library.Current.QueryMe(ViewMode.Table);

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

        public List<PlaylistItem> GetSelected()
        {
            if (TrackTable.SelectedItem == null)
                return null;

            List<PlaylistItem> plItems = new List<PlaylistItem>();
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

            var vm = DataContext as TrackTableViewModel;
            var albumItems = LibraryUtils.GetAlbumItems(item, vm.LibraryTracks.ToList());
            PlaylistViewModel.Instance.Add(albumItems);
        }

        private void mnuCtxInsertAlbum_Click(object sender, RoutedEventArgs e)
        {
            var item = GetFirstSelected();
            if (item == null)
                return;

            var vm = DataContext as TrackTableViewModel;
            var albumItems = LibraryUtils.GetAlbumItems(item, vm.LibraryTracks.ToList());
            PlaylistViewModel.Insert(albumItems);
        }

        private void mnuCtxPlayAlbum_Click(object sender, RoutedEventArgs e)
        {
            var item = GetFirstSelected();
            if (item == null)
                return;

            var vm = DataContext as TrackTableViewModel;
            var albumItems = LibraryUtils.GetAlbumItems(item, vm.LibraryTracks.ToList());
            PlaylistViewModel.Play(albumItems);
        }

        private void mnuCtxPlayAll_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as TrackTableViewModel;
            var uniques = vm.Tracks.DistinctBy(x => x.Title).ToArray();
            PlaylistViewModel.Play(uniques);
        }
    }
}

using Classes;
using Models;
using Utilities;
using ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MpFree4k;

namespace Controls
{
    public partial class Playlist : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private List<PlaylistInfo> dragItems = new List<PlaylistInfo>();
        private PlaylistViewModel vm = null;
        private System.Timers.Timer addCompleteTimer;
        private Thread t_addFiles;
        private bool drag_invalidate = false;
        private bool mousedown = false;
        private Point mousepos = new Point(0, 0);

        public Playlist()
        {
            vm = new PlaylistViewModel();
            DataContext = vm;
            vm.PropertyChanged += Playlist_PropertyChanged;
            MainWindow.Instance.SmartPlayer.PropertyChanged += Player_PropertyChanged;
            MainWindow.Instance.SmartPlayer.ValueChanged += Player_ValueChanged;
            Loaded += Playlist_Loaded;
            SizeChanged += Playlist_SizeChanged;

            InitializeComponent();

            addCompleteTimer = new System.Timers.Timer(100);
            addCompleteTimer.Elapsed += AddCompleteTimer_Elapsed;
        }

        private double _availableWidth = 0;
        public double AvailableWidth
        {
            get => _availableWidth;
            set
            {
                _availableWidth = value;
                Raise(nameof(AvailableWidth));
            }
        }

        private void AddCompleteTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (t_addFiles == null || !t_addFiles.IsAlive)
            {
                addCompleteTimer.Stop();
                vm.FinalizeAdd();

                Dispatcher.Invoke(() =>
                {
                    PlaylistView.ItemsSource = null;
                    PlaylistView.ItemsSource = vm.Tracks;
                    Raise("Tracks");
                });

                MainWindow.SetProgress(0);
            }
        }

        private void Player_ValueChanged(object sender, ValueChangedEvent e)
        {
            if (e.Key == "PlayTimeUpdate")
            {
                double progress = Convert.ToDouble(e.Value);
                vm.StatusVM.Update(progress);
            }
        }

        private void Playlist_SizeChanged(object sender, SizeChangedEventArgs e) => setWidth();
        private void Playlist_Loaded(object sender, RoutedEventArgs e) => setWidth();
        private void PlaylistView_ScrollChanged(object sender, ScrollChangedEventArgs e) => setWidth();

        void setWidth()
        {
            AvailableWidth = PlaylistView.ActualWidth;
            Decorator border = System.Windows.Media.VisualTreeHelper.GetChild(PlaylistView, 0) as Decorator;
            if (border != null)
            {
                ScrollViewer scroller = border.Child as ScrollViewer;
                if (scroller != null)
                {
                    Visibility scrollbarVisibility = scroller.ComputedVerticalScrollBarVisibility;
                    double substract = SystemParameters.VerticalScrollBarWidth;
                    double newwid = 0;
                    if (scrollbarVisibility != Visibility.Collapsed)
                        newwid = scroller.ActualWidth - substract - 4;
                    else
                        newwid = scroller.ActualWidth - 4;

                    if (AvailableWidth != newwid)
                        AvailableWidth = newwid;
                }
            }
        }

        private void Player_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PlaylistViewModel VM = this.DataContext as PlaylistViewModel;
            if (VM.CurrentSong == null)
                return;

            PlaylistInfo p = VM.CurrentSong;
            PlaylistView.ScrollIntoView(p);

            if (e.PropertyName == "Play")
                VM.StatusVM.Update();
        }

        private void Playlist_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tracks")
            {
                PlaylistView.ItemsSource = null;
                PlaylistView.ItemsSource = vm.Tracks;
            }
            else if (e.PropertyName == "Play")
            {
                PlaylistViewModel VM = this.DataContext as PlaylistViewModel;
                if (VM.CurrentSong == null)
                    return;

                PlaylistInfo p = VM.CurrentSong;
                PlaylistView.ScrollIntoView(p);
            }
        }

        private void PlaylistView_DragOver(object sender, DragEventArgs e)
        {
            if (!(e.OriginalSource is FrameworkElement))
                return;

            FrameworkElement element = (FrameworkElement)e.OriginalSource;
            if (element == null)
                return;

            ListViewItem lvi = (ListViewItem)PlaylistView.ItemContainerGenerator.ContainerFromItem(element.DataContext);
            if (lvi != null)
                (lvi.DataContext as PlaylistInfo).DragOver = true;
        }

        private void PlaylistView_DragLeave(object sender, DragEventArgs e)
        {
            if (!(e.OriginalSource is FrameworkElement))
                return;

            FrameworkElement element = (FrameworkElement)e.OriginalSource;
            if (element == null)
                return;

            ListViewItem lvi = (ListViewItem)PlaylistView.ItemContainerGenerator.ContainerFromItem(element.DataContext);
            if (lvi != null)
                (lvi.DataContext as PlaylistInfo).DragOver = false;
        }

        private void PlaylistView_Drop(object sender, DragEventArgs e)
        {
            if (!(e.OriginalSource is FrameworkElement))
                return;

            FrameworkElement element = (FrameworkElement)e.OriginalSource;
            if (element == null)
                return;

            ListViewItem lvi = null;
            int pos = vm.Tracks.Count;

            lvi = (ListViewItem)PlaylistView.ItemContainerGenerator.ContainerFromItem(element.DataContext);
            if (lvi != null)
                pos = (lvi.DataContext as PlaylistInfo).Position - 1;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                addDropFiles(files, pos);
                dragItems.Clear();

                return;
            }
            else if (e.Data.GetDataPresent("MediaLibraryItemData"))
            {
                IList data = e.Data.GetData("MediaLibraryItemData") as IList;
                if (data == null || data.Count == 0)
                    return;

                List<PlaylistInfo> infoItms = (from object item in data select (PlaylistInfo)item).ToList();
                dragItems.Clear();

                vm.Add(infoItms, pos);
                vm.StatusVM.Update();

                return;

            }
            else if (e.Data.GetDataPresent("PlaylistItemData"))
            {
                if (drag_invalidate)
                    return;

                IList data = e.Data.GetData("PlaylistItemData") as IList;
                if (data == null || data.Count == 0)
                    return;

                List<PlaylistInfo> infoItems = (from object item in data select (PlaylistInfo)item).ToList();

                if (dragItems.Any(x => x._position == pos + 1))
                {
                    vm.UnDrag();
                    return;
                }

                vm.Move(infoItems, pos);
                dragItems.Clear();
                vm.StatusVM.Update();

                return;

            }
            else if (e.Data.GetDataPresent("MediaLibraryAlbumData"))
            {
                IList data = e.Data.GetData("MediaLibraryAlbumData") as IList;
                if (data.Count <= 0)
                    return;

                var albums = data.Cast<AlbumItem>();

                t_addFiles = new Thread(() =>
                {
                    int firstpos = pos;
                    double step = 100.0 / (double)albums.Sum(a => a.Tracks.Count);
                    double progress = 0;

                    MainWindow.SetProgress(3);

                    foreach (AlbumItem album in albums)
                        foreach (PlaylistInfo pli in LibraryUtils.GetItems(album.Tracks.ToArray()))
                        {
                            progress += step;
                            MainWindow.SetProgress(progress);

                            vm.Add(pli, pos++);
                        }

                    dragItems.Clear();

                });

                t_addFiles.Start();
                addCompleteTimer.Start();
            }
            else if (e.Data.GetDataPresent("MediaLibraryFileInfos"))
            {
                IList data = e.Data.GetData("MediaLibraryFileInfos") as IList;
                if (data.Count <= 0)
                    return;

                var info = data.Cast<FileViewInfo>();
                List<PlaylistInfo> playlistItems = new List<PlaylistInfo>();

                foreach (var it in info)
                {
                    PlaylistInfo pli = PlaylistHelpers.CreateFromMediaItem(it);
                    playlistItems.Add(pli);
                }

                vm.Add(playlistItems, pos);
                vm.StatusVM.Update();

                return;
            }
            else if (e.Data.GetDataPresent("AlbumItemData"))
            {
                IList data = e.Data.GetData("AlbumItemData") as IList;
                if (data.Count <= 0)
                    return;

                SimpleAlbumItem album = data[0] as SimpleAlbumItem;

                t_addFiles = new Thread(() =>
                {
                    int firstpos = pos;
                    double step = 100.0 / (double)album.TrackCount;
                    double progress = 0;

                    MainWindow.SetProgress(3);

                    foreach (PlaylistInfo pli in LibraryUtils.GetItems(album.Tracks.ToArray()))
                    {
                        progress += step;
                        MainWindow.SetProgress(progress);

                        vm.Add(pli, pos++);
                    }

                    dragItems.Clear();

                });
            }
            else
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length == 0)
                    return;

                List<PlaylistInfo> externalItems = new List<PlaylistInfo>();

                foreach (string file in files)
                    externalItems.Add(PlaylistHelpers.CreateFromFileViewInfo(new FileViewInfo(file)));

                vm.Add(externalItems, pos);
                vm.StatusVM.Update();

                return;
            }

            t_addFiles.Start();
            addCompleteTimer.Start();
        }

        private void PlaylistView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            mousedown = true;
            mousepos = e.MouseDevice.GetPosition(this);

            var item = ItemsControl.ContainerFromElement(PlaylistView, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                if (!MainWindow.ctrl_down)
                {
                    if (item.IsSelected)
                        e.Handled = true;
                    else
                    {
                        PlaylistView.SelectedItems.Clear();
                        item.IsSelected = true;
                    }
                }
                else
                {
                    if (item.IsSelected)
                    {
                        if (PlaylistView.SelectedItems.Contains(item))
                            PlaylistView.SelectedItems.Remove(item);
                        else
                            PlaylistView.SelectedItems.Add(item);
                    }
                }

            }
        }

        void addDropFiles(string[] files, int pos)
        {
            List<PlaylistInfo> plItems = new List<PlaylistInfo>();
            foreach (var file in files)
            {
                var ext = System.IO.Path.GetExtension(file);
                if (!Configuration.Config.media_extensions.Contains(ext))
                    continue;

                FileViewInfo f_Sel = new FileViewInfo(file);
                PlaylistInfo p_i = new PlaylistInfo();
                PlaylistHelpers.CreateFromMediaItem(p_i, f_Sel);
                plItems.Add(p_i);
            }

            vm.Add(plItems, pos);
            vm.StatusVM.Update();
        }


        private void PlaylistView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                delete_selected();
            }
        }

        private void delete_selected()
        {
            if (PlaylistView.SelectedItems == null ||
                PlaylistView.SelectedItems.Count == 0)
                return;

            int[] remove_indices = new int[PlaylistView.SelectedItems.Count];
            int idx = 0;
            foreach (PlaylistInfo infoItm in PlaylistView.SelectedItems)
            {
                remove_indices[idx] = infoItm._position - 1;
                idx++;
            }
            vm.Remove(remove_indices.OrderBy(x => x).ToArray());

            vm.StatusVM.Update();
        }

        private void PlaylistView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dragItems.Clear();

            foreach (PlaylistInfo pli in PlaylistView.SelectedItems) dragItems.Add(pli);
        }

        private void TrackClick(object sender, MouseButtonEventArgs e)
        {
            PlaylistInfo pi = (sender as ListViewItem).DataContext as PlaylistInfo;
            vm.CurrentPlayPosition = pi._position - 1;
            vm.Invoke(PlayState.PlayFromStart);

            dragItems.Clear();

            e.Handled = true;
        }

        private void PlaylistView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            drag_invalidate = true;
            mousedown = false;
            mousepos = new Point(0, 0);
        }

        private void PlaylistView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mousepos.X != 0 && mousepos.Y != 0 && mousedown && e.LeftButton == MouseButtonState.Pressed && e.OriginalSource is TextBlock)
            {
                drag_invalidate = false;
                DragDrop.DoDragDrop(PlaylistView, new DataObject("PlaylistItemData", dragItems), DragDropEffects.Move);
            }
        }

        private void mnuCtxSelectAll_Click(object sender, RoutedEventArgs e)
        {
            PlaylistView.SelectAll();
        }

        private void mnuCtxRemove_Click(object sender, RoutedEventArgs e)
        {
            delete_selected();
        }

        private void mnuCtxPlay_Click(object sender, RoutedEventArgs e)
        {
            PlaylistInfo pi = PlaylistView.SelectedItem as PlaylistInfo;
            if (pi == null) return;

            vm.CurrentPlayPosition = pi._position - 1;
            vm.Invoke(PlayState.PlayFromStart);
            dragItems.Clear();
            e.Handled = true;
        }

        private void mnuCtxUnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            PlaylistView.UnselectAll();
        }

        private void mnuCtxEditItem_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistView.SelectedItems == null) return;
            var selected = new List<PlaylistInfo>();

            foreach (PlaylistInfo pli in PlaylistView.SelectedItems) selected.Add(pli);
            FileViewInfo[] infos = TracksViewModel.Instance.Tracks.Where(t => selected.Any(s => s.Path.Equals(t.Path))).ToArray();

            if (infos.Length != selected.Count)
                MessageBox.Show("Some of the selected Tracks could not be found in the Library");

            if (infos.Length == 0) return;
            Dialogs.TracksEditor editor = new Dialogs.TracksEditor(infos);
            editor.ShowDialog();
        }
    }
}

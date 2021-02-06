using Classes;
using MpFree4k.Classes;
using MpFree4k.Dialogs;
using MpFree4k.Enums;
using MpFree4k.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ViewModels;

namespace MpFree4k.Controls
{
    public partial class TrackView : UserControl, INotifyPropertyChanged
    {
        List<FileViewInfo> dragItems = new List<FileViewInfo>();

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public void SetMediaLibrary(Layers.MediaLibrary lib)
        {
            (this.DataContext as TracksViewModel).MediaLibrary = lib;
        }

        public TrackView()
        {
            this.DataContext = new TracksViewModel();
            (this.DataContext as TracksViewModel).PropertyChanged += TrackView_PropertyChanged;
            this.SizeChanged += TrackView_SizeChanged;
            InitializeComponent();
        }

        private void TrackView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListWidth = calcListWidth();
        }

        private void TrackView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tracks")
                ListWidth = calcListWidth();
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
 where childItem : DependencyObject
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
            ListView view = ListTracks;
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
            set
            {
                _listWidth = value;
                OnPropertyChanged("ListWidth");
            }
            get
            {

                return _listWidth;

            }
        }

        private List<FileViewInfo> GetSelectedFileViewInfos()
        {
            return ListTracks.SelectedItems.Cast<FileViewInfo>().ToList().Where(v => v.IsVisible).ToList();
        }

        private List<PlaylistItem> GetSelectedPlaylistItems()
        {
            List<PlaylistItem> items = new List<PlaylistItem>();
            foreach (var fi in GetSelectedFileViewInfos())
            {
                PlaylistItem item = new PlaylistItem();
                PlaylistHelpers.CreateFromFileViewInfo(item, fi);
                items.Add(item);
            }
            return items;
        }

        private void ListTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dragItems.Clear();
            dragItems.AddRange(GetSelectedFileViewInfos());
        }

        private void ListTracks_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mousepos.X != 0 && mousepos.Y != 0 && mousedown && e.LeftButton == MouseButtonState.Pressed && e.OriginalSource is TextBlock)
            {
                Point curpos = e.MouseDevice.GetPosition(this);
                double x = Math.Abs(mousepos.X - curpos.X);
                double y = Math.Abs(mousepos.Y - curpos.Y);
                if (x < Config.drag_pixel || y < Config.drag_pixel)
                    return;

                DragDrop.DoDragDrop(ListTracks, new DataObject("MediaLibraryFileInfos", dragItems), DragDropEffects.Move);
            }
        }

        bool ctrlkey_down = false;
        private void _This_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl ||
              e.Key == Key.RightCtrl)
                ctrlkey_down = true;
        }

        private void _This_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl ||
                e.Key == Key.RightCtrl)
                ctrlkey_down = false;
        }

        private void ListTracks_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            mousedown = true;
            mousepos = e.MouseDevice.GetPosition(this);

            var item = ItemsControl.ContainerFromElement(ListTracks, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                if (!MainWindow.ctrl_down)
                {
                    if (item.IsSelected)
                        e.Handled = true;
                    else
                    {
                        ListTracks.SelectedItems.Clear();
                        (this.DataContext as TracksViewModel).Tracks.ForEach(t => t.IsSelected = false);
                        item.IsSelected = true;
                    }
                }
                else
                {
                    if (item.IsSelected)
                    {
                        if (ListTracks.SelectedItems.Contains(item))
                            ListTracks.SelectedItems.Remove(item);
                        else
                            ListTracks.SelectedItems.Add(item);
                    }
                    //item.IsSelected = !item.IsSelected;
                }

            }

        }

        private void mnuCtxEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ListTracks.SelectedItems == null ||
                ListTracks.SelectedItems.Count == 0)
                return;

            FileViewInfo[] infos = ListTracks.SelectedItems.Cast<FileViewInfo>().ToArray();
            TracksEditor editor = new TracksEditor(infos);
            editor.ShowDialog();
        }

        private TrackViewType _trackViewType = Enums.TrackViewType.List;
        public TrackViewType TrackViewType
        {
            get { return _trackViewType; }
            set
            {
                _trackViewType = value;
                OnPropertyChanged("TrackViewType");
            }
        }

        public void SetViewType(TrackViewType type)
        {
            TrackViewType = type;
            UserConfig.TrackViewType = type;
        }

        private void ListTracks_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FileViewInfo f_Sel = ListTracks.SelectedItem as FileViewInfo;
            PlaylistItem p_i = new PlaylistItem();
            PlaylistHelpers.CreateFromMediaItem(p_i, f_Sel);
            PlaylistViewModel VM = (MainWindow._singleton).Playlist.DataContext as PlaylistViewModel;
            int playpos = VM.CurrentPlayPosition;

            if (VM.Tracks.Count > 0)
            {
                if (VM.Tracks.Count < playpos)
                    playpos = VM.Tracks.Count;
                else
                    playpos++;
            }
            else
                playpos = 0;

            VM.Add(new List<PlaylistItem>() { p_i }, playpos);
            if (playpos >= VM.Tracks.Count)
                playpos = VM.Tracks.Count - 1;

            if (playpos < 0)
                return;

            PlaylistItem cloned = VM.Tracks[playpos];
            VM.enumerate(playpos);
            VM.CurrentPlayPosition = cloned._position - 1;
            (MainWindow._singleton.Playlist.DataContext as PlaylistViewModel).Invoke(PlayState.Play);
        }

        private bool mousedown = false;
        Point mousepos = new Point(0, 0);
        private void ListTracks_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            mousedown = false;
            mousepos = new Point(0, 0);
        }

        private void ListTracks_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (MainWindow.ctrl_down && e.Key == Key.A)
            {
                TracksViewModel VM = this.DataContext as TracksViewModel;
                VM.Tracks.Where(a => a.IsVisible).ToList().ForEach(x => x.IsSelected = true);
                VM.MediaLibrary.Refresh(Layers.MediaLevel.All);
                e.Handled = true;
            }
        }

        private void ListTracks_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double wid = calcListWidth();
            if (ListWidth != wid)
                ListWidth = wid;
        }

        private void mnuCtxPlay_Click(object sender, RoutedEventArgs e)
        {
            if (ListTracks.SelectedItems == null || ListTracks.SelectedItems.Count == 0)
                return;

            PlaylistViewModel.Play(GetSelectedPlaylistItems().ToArray());
        }

        private void mnuCtxInsert_Click(object sender, RoutedEventArgs e)
        {
            if (ListTracks.SelectedItems == null || ListTracks.SelectedItems.Count == 0)
                return;

            PlaylistViewModel.Insert(GetSelectedPlaylistItems().ToArray());

        }

        private void mnuCtxAdd_Click(object sender, RoutedEventArgs e)
        {
            if (ListTracks.SelectedItems == null || ListTracks.SelectedItems.Count == 0)
                return;

            PlaylistViewModel.Add(GetSelectedPlaylistItems().ToArray());

        }
    }
}

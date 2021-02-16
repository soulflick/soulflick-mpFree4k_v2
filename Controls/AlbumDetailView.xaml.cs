using Classes;
using MpFree4k.Classes;
using MpFree4k.Dialogs;
using MpFree4k.Enums;
using MpFree4k.Utilies;
using MpFree4k.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MpFree4k.Controls
{
    public static class ViewportHelper
    {
        public static bool IsInViewport(Control item)
        {
            if (item == null) return false;
            ItemsControl itemsControl = null;
            if (item is ListBoxItem)
            {
                itemsControl = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
            }
            else
            {
                throw new NotSupportedException(item.GetType().Name);
            }

            ScrollViewer scrollViewer = LocalVisualTreeHelper.GetVisualChild<ScrollViewer, ItemsControl>(itemsControl);
            ScrollContentPresenter scrollContentPresenter = (ScrollContentPresenter)scrollViewer.Template.FindName("PART_ScrollContentPresenter", scrollViewer);
            MethodInfo isInViewportMethod = scrollViewer.GetType().GetMethod("IsInViewport", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)isInViewportMethod.Invoke(scrollViewer, new object[] { scrollContentPresenter, item });
        }
    }
    public static class LocalVisualTreeHelper
    {
        private static void GetVisualChildren<T>(DependencyObject current, Collection<T> children) where T : DependencyObject
        {
            if (current != null)
            {
                if (current.GetType() == typeof(T))
                {
                    children.Add((T)current);
                }

                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    GetVisualChildren<T>(System.Windows.Media.VisualTreeHelper.GetChild(current, i), children);
                }
            }
        }

        public static Collection<T> GetVisualChildren<T>(DependencyObject current) where T : DependencyObject
        {
            if (current == null)
            {
                return null;
            }

            Collection<T> children = new Collection<T>();

            GetVisualChildren<T>(current, children);

            return children;
        }

        public static T GetVisualChild<T, P>(P templatedParent)
            where T : FrameworkElement
            where P : FrameworkElement
        {
            Collection<T> children = LocalVisualTreeHelper.GetVisualChildren<T>(templatedParent);

            foreach (T child in children)
            {
                if (child.TemplatedParent == templatedParent)
                {
                    return child;
                }
            }

            return null;
        }
    }


    /// <summary>
    /// Interaktionslogik für Favourites.xaml
    /// </summary>
    public partial class AlbumDetailView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        // FavouriteAlbums

        public AlbumDetailViewModel VM = null;

        public AlbumDetailView()
        {
            VM = new AlbumDetailViewModel();

            this.DataContext = VM;

            this.Loaded += Favourites_Loaded;

            InitializeComponent();

        }

        private AlbumDetailsOrderType _albumDetailsOrderType = Enums.AlbumDetailsOrderType.Year;
        public AlbumDetailsOrderType AlbumDetailsOrderType
        {
            get { return _albumDetailsOrderType; }
            set
            {
                _albumDetailsOrderType = value;
                VM.OrderBy(_albumDetailsOrderType);
                OnPropertyChanged("AlbumDetailsOrderType");
            }
        }

        private void Favourites_Loaded(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.ViewMode != ViewMode.Albums)
                return;

            if (MainWindow.Instance.MainViews.SelectedIndex == (int)TabOrder.Albums)
                VM.Reload();

            if (gridCharTip != null)
                gridCharTip.Visibility = AlbumDetailsOrderType != AlbumDetailsOrderType.All ? Visibility.Visible : Visibility.Hidden;
        }

        List<AlbumItem> dragItems_albums = new List<AlbumItem>();

        private bool mousedown_albums = false;
        Point mousepos = new Point(0, 0);
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

                DragDrop.DoDragDrop(sender as ListView, new DataObject("MediaLibraryAlbumData", dragItems_albums), DragDropEffects.Move);
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

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab.Tag is AlbumDetailsOrderType)
            {
                AlbumDetailsOrderType = (AlbumDetailsOrderType)(sender as Label).Tag;
                gridCharTip.Visibility = AlbumDetailsOrderType != AlbumDetailsOrderType.All ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private string _currentCharacter = "A";
        public string CurrentCharacter
        {
            get { return _currentCharacter; }
            set
            {
                _currentCharacter = value;
                OnPropertyChanged("CurrentCharacter");
            }
        }

        private bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds.TopLeft) || rect.Contains(bounds.BottomRight);
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (ListGroups.Items.Count == 0)
                return;

            for (int i = 0; i < ListGroups.Items.Count; i++)
            {
                var item = ListGroups.ItemContainerGenerator.ContainerFromIndex(i);

                ContentPresenter contentPresenter = ListGroups.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                Panel element = contentPresenter.ContentTemplate.LoadContent() as Panel;

                Rect bounds = contentPresenter.TransformToAncestor(ListGroups).TransformBounds(new Rect(0.0, 0.0, contentPresenter.ActualWidth, contentPresenter.ActualHeight));
                Rect rect = new Rect(0.0, 0.0, ListGroups.ActualWidth, ListGroups.ActualHeight);
                bool vis = IsUserVisible(contentPresenter, mainGrid);

                if (vis)
                {
                    AlbumDetailGroup grp = ListGroups.Items[i] as AlbumDetailGroup;
                    if (string.IsNullOrEmpty(grp.GroupName))
                        CurrentCharacter = "";
                    else
                        CurrentCharacter = grp.JumpNode;
                    break;
                }
            }
        }

        public PlaylistItem[] GetSelectedTracks()
        {
            var album = dragItems_albums.FirstOrDefault();
            if (album == null)
                return null;

            return LibraryUtils.GetTracks(album);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer)sender;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        }

       
        private void mnuCtxPlay_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).DataContext is AlbumItem album)
            {
                PlaylistViewModel.Play(LibraryUtils.GetTracks(album));
            }
        }

        private void mnuCtxInsert_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).DataContext is AlbumItem album)
            {
                PlaylistViewModel.Insert(LibraryUtils.GetTracks(album));
            }
        }

        private void mnuCtxAdd_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).DataContext is AlbumItem album)
            {
                PlaylistViewModel.Add(LibraryUtils.GetTracks(album));
            }
        }

        private void menuCtxEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).DataContext is AlbumItem album)
            {
                FileViewInfo[] infos = LibraryUtils.GetInfoItems(album);

                TracksEditor editor = new TracksEditor(infos);
                editor.ShowDialog();
            }
        }

        List<ListView> Lists = new List<ListView>();
        private void ListAlbums_Loaded(object sender, RoutedEventArgs e)
        {
            ListView v = sender as ListView;
            if (!Lists.Contains(v))
                Lists.Add(v);
        }

        private void ListAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView v = sender as ListView;
            Lists.Where(l => l != v && l.SelectedItems != null).ToList().ForEach(l => l.SelectedItem = null);

            mousedown_albums = true;

            dragItems_albums.Clear();
            dragItems_albums.AddRange(v.SelectedItems.Cast<AlbumItem>());
        }
    }
}

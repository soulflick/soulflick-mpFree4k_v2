using MpFree4k.Classes;
using MpFree4k.Dialogs;
using MpFree4k.Enums;
using MpFree4k.Layers;
using MpFree4k.ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.ComponentModel;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;

namespace MpFree4k
{
    public enum ViewMode
    {
        Details,
        Table,
        Albums,
        Favourites
    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public void UpdateHeaderSize()
        {
        }

        private ViewMode _viewMode = ViewMode.Details;
        public ViewMode ViewMode
        {
            get { return _viewMode; }
            set
            {
                _viewMode = value;
                OnViewMode();
            }
        }

        private void OnViewMode()
        {
            if (_viewMode == ViewMode.Albums)
            {
                Controls.AlbumView.StaticViewModel.UpdateAmount();
            }
        }

        public static MainWindow _singleton = null;
        public static void SetProgress(double percent)
        {
            if (_singleton == null)
                return;

            mainDispatcher.BeginInvoke(_singleton.delegateUpdateProgress, new object[] { percent });
        }

        public delegate void updateProgress(double percent);
        public updateProgress delegateUpdateProgress;

        public void updateProgressFunc(double percent)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgressBar.Value = percent;
                ProgressBar.UpdateLayout();
                IsEnabled = percent == 0;
            }));
        }


        public static Dispatcher mainDispatcher = null;
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        Library _library = null;
        public Library Library
        {
            get { return _library; }
            set
            {
                if (_library != null && _library.Current != null)
                    _library.Current.PropertyChanged -= Current_PropertyChanged;

                _library = value;

                if (_library != null && _library.Current != null)
                    _library.Current.PropertyChanged += Current_PropertyChanged;

                OnPropertyChanged("Library");
            }
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (new string[] { "Albums", "Tracks", "Filter" }.Contains(e.PropertyName))
            {
                if (Library != null && Library.Current != null)
                {
                    int num_tracks = Library.Current.GetVisibleTracks();
                    TimeSpan length = Library.Current.GetVisibleLength();
                    SetAmounts(num_tracks, length);
                }
            }
        }

        public void SetAmounts(int count, TimeSpan duration)
        {
            string content = "length: " + duration.ToString() + " count: " + count.ToString();
            this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => tbAmountFiltered.Text = content));
        }


        private RepeatMode _repeatMode = RepeatMode.GoThrough;
        public RepeatMode RepeatMode { get { return _repeatMode; } set { _repeatMode = value; OnPropertyChanged("RepeatMode"); } }

       
        public SettingControl setctrl = null;
        PlaylistSelector plsel = null;

        public MainWindow()
        {
            _singleton = this;
            setctrl = new SettingControl();

            ViewMode = ViewMode.Table;

            Library = new Library();
           
            InitializeComponent();

            if (!setctrl.HasConfig)
            {
                setctrl.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                setctrl.ShowDialog();
            }

            SkinAdaptor.ApplySkin(this, UserConfig.Skin, UserConfig.FontSize);

            plsel = new PlaylistSelector(Playlist.DataContext as PlaylistViewModel);
            if (plsel.SelectedDefinition != null)
                PlaylistSerializer.Load(Playlist.DataContext as PlaylistViewModel, plsel.SelectedDefinition.Path);

            mainDispatcher = this.TableView.AlbumView.Dispatcher;
            this.TableView.AlbumView.SetMediaLibrary(Library.Current);
            this.TableView.ArtistView.SetMediaLibrary(Library.Current);
            this.TableView.TrackView.SetMediaLibrary(Library.Current);
            this.TrackTable.SetMediaLibrary(Library.Current);
            this.Player.PlayListVM = Playlist.DataContext as ViewModels.PlaylistViewModel;

            delegateUpdateProgress = new updateProgress(updateProgressFunc);

            TableView.ArtistView.SetViewType(UserConfig.ArtistViewType);
            TableView.AlbumView.SetViewType(UserConfig.AlbumViewType);
            TableView.TrackView.SetViewType(UserConfig.TrackViewType);

            TableView.ArtistViewType = UserConfig.ArtistViewType;
            TableView.AlbumViewType = UserConfig.AlbumViewType;
            TableView.TrackViewType = UserConfig.TrackViewType;

            Library.Load();

            query_timer.AutoReset = false;
            query_timer.Elapsed += Query_timer_Elapsed;

            this.Width += 1;
            
        }

        private void Query_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Library.Current.ClearSelection();
            Library.Current._query = _query;
            Library.Current.QueryMe(ViewMode);
            query_timer.Stop();
        }

        private string _query = "";
        Timer query_timer = new Timer(300);
        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _query = (sender as TextBox).Text;
            query_timer.Stop();
            query_timer.Start();

        }

        double _headerHeight = 0;
        public double HeaderHeight
        {
            get
            {
                return _headerHeight;
            }
            set
            {
                _headerHeight = value;
                OnPropertyChanged("HeaderHeight");
            }
        }

        private double _progressValue = 0;
        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                OnPropertyChanged("ProgressValue");
            }
        }

        public static bool ctrl_down = false;
        private void _This_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                ctrl_down = true;
            else if (e.Key == Key.F12)
            {
                if (this.WindowStyle != WindowStyle.None)
                {
                    this.WindowStyle = WindowStyle.None;
                }
                else
                {
                    this.WindowStyle = WindowStyle.ThreeDBorderWindow;
                }
            }
        }

        private void _This_KeyUp(object sender, KeyEventArgs e)
        {
            if (ctrl_down && e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                ctrl_down = false;
        }

        private void _This_Closing(object sender, CancelEventArgs e)
        {
            SettingControl.WriteUserConfig();
            if (UserConfig.AutoSavePlaylist)
            {
                plsel = new PlaylistSelector(this.Playlist.DataContext as PlaylistViewModel);
                if (plsel.SelectedDefinition != null)
                {
                    PlaylistSerializer.Serialize(plsel.SelectedDefinition.Path, (this.Playlist.DataContext as PlaylistViewModel).Tracks);
                }
            }

            this.Player.Shutdown();

            Application.Current.Shutdown();
        }

        private void RepeatMode_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lab = sender as Label;
            if (lab.Tag is RepeatMode)
            {
                RepeatMode mode = (RepeatMode)(sender as Label).Tag;
                RepeatMode = mode;
                (Playlist.DataContext as PlaylistViewModel).RepeatMode = mode;
            }
        }

        private void PlaylistManagement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PlaylistSelector selector = new PlaylistSelector(Playlist.DataContext as PlaylistViewModel);
            selector.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            selector.ShowDialog();
        }

        private string getCountString(int count)
        {
            string cnt = count.ToString();
            int cnt_len = cnt.Length;
            for (int i = 0; i < 9 - cnt_len; i++)
                cnt = "0" + cnt;
            return cnt;
        }

        private void ExportPlaylist_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "please select an export location";

            CommonFileDialogResult result = dialog.ShowDialog();

            int pos = 0;
            if (result == CommonFileDialogResult.Ok)
            {
                string location = dialog.FileName;
                int count = 0;
                foreach (PlaylistItem pi in (Playlist.DataContext as PlaylistViewModel).Tracks)
                {
                    if (!File.Exists(pi.Path))
                        continue;

                    pos++;
                    try
                    {

                        string target = Path.Combine(location, getCountString(pos) + "__" + Path.GetFileName(pi.Path));
                        File.Copy(pi.Path, target);
                        count++;
                    }
                    catch { continue; }
                }

                MessageBox.Show(count.ToString() + " files copied to " + location);
            }

            this.BringIntoView();
        }

        private void _This_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width += 1;
        }

        private void MainViews_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((sender as TabControl).SelectedIndex)
            {
                case (int)TabOrder.Detail: ViewMode = ViewMode.Details; break;
                case (int)TabOrder.Tracks: ViewMode = ViewMode.Table; break;
                case (int)TabOrder.Albums: ViewMode = ViewMode.Albums; break;
                case (int)TabOrder.Favourites: ViewMode = ViewMode.Favourites; break;
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Maximized) ?
                WindowState.Normal : WindowState.Maximized;
        }

        private void btnEqualizer_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EqualizerWindow eq = new EqualizerWindow();
            eq.ShowDialog();
        }

        private void btnSettings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SettingControl settctrl = new SettingControl();
            settctrl.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            settctrl.ShowDialog();
            if (ViewMode == ViewMode.Favourites)
            {
                Favourites.Reload();
            }
        }

        private void btnLibraries_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LibrarySelector selector = new LibrarySelector();
            selector.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            selector.ShowDialog();
            if (selector.DialogSelection != null)
                Library.LoadFrom(selector.DialogSelection);
        }

        private void btnInfo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            InfoScreen info = new InfoScreen();
            info.ShowDialog();
        }
    }
}

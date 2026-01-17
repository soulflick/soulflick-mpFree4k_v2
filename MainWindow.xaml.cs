using Classes;
using Dialogs;
using Mpfree4k.Enums;
using Layers;
using ViewModels;
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
using Models;
using Configuration;
using System.Text;

namespace MpFree4k
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public enum MainTabs
        {
            Artists, Albums, Tracks, Favourites
        }

        public static MainWindow Instance = null;
        public SettingControl setctrl = null;
        public delegate void updateProgress(double percent);
        public updateProgress delegateUpdateProgress;
        public static bool ctrl_down = false;

        private PlaylistSelector plsel = null;
        public static Dispatcher mainDispatcher = null;
        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private string _query = "";
        public string FilterHint => "type to search for tracks";

        Timer query_timer = new Timer(300);

        private ViewMode _viewMode = ViewMode.Details;
        public ViewMode ViewMode
        {
            get => _viewMode;
            set
            {
                _viewMode = value;
                OnViewMode();
            }
        }


        double _headerHeight = 0;
        public double HeaderHeight
        {
            get => _headerHeight;
            set
            {
                _headerHeight = value;
                Raise(nameof(HeaderHeight));
            }
        }

        private double _progressValue = 0;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                Raise(nameof(ProgressValue));
            }
        }

        private RepeatMode _repeatMode = RepeatMode.GoThrough;
        public RepeatMode RepeatMode
        {
            get => _repeatMode;
            set { _repeatMode = value; Raise(nameof(RepeatMode)); }
        }

        Library _library = null;
        public Library Library
        {
            get => _library;
            set
            {
                if (_library != null && _library.Current != null)
                    _library.Current.PropertyChanged -= Current_PropertyChanged;

                _library = value;

                if (_library != null && _library.Current != null)
                    _library.Current.PropertyChanged += Current_PropertyChanged;

                Raise(nameof(Library));
            }
        }

        private void OnViewMode()
        {
            if (_viewMode == ViewMode.Albums)
                AlbumsViewModel.Instance.UpdateAmount();
        }

        public static void SetProgress(double percent)
        {
            if (Instance == null) return;

            mainDispatcher?.BeginInvoke(Instance.delegateUpdateProgress, new object[] { percent });
        }

        public void updateProgressFunc(double percent)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ProgressBar.Value = percent;
                ProgressBar.UpdateLayout();
                Controls.SmartPlayer.Instance.SetEnabled(percent == 0);
            }));
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (new string[] { "Albums", "Tracks", "Filter" }.Contains(e.PropertyName))
            {
                if (Library != null && Library.Current != null)
                {
                    SetAmounts();
                }
            }
        }

        public void SetAmounts()
        {
            int num_tracks = Library.Current.GetVisibleTracks();
            TimeSpan length = Library.Current.GetVisibleLength();
            SetAmounts(num_tracks, length);
        }

        public void SetAmounts(int count, TimeSpan duration)
        {
            string content = "length: " + duration.ToString() + " count: " + count.ToString();
            this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() => tbAmountFiltered.Text = content));
        }

        public MainWindow()
        {
            Instance = this;

            setctrl = new SettingControl();

            ViewMode = ViewMode.Table;

            Library = new Library();
           
            InitializeComponent();

            if (!setctrl.HasConfig)
            {
                setctrl.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                setctrl.ShowDialog();
            }
            Classes.StandardImage.Reload();
            SkinAdaptor.ApplySkin(this, UserConfig.Skin, UserConfig.FontSize);
            SkinAdaptor.ApplyPadding(this, UserConfig.PaddingType);
            PlaylistViewModel.Instance.ShowPathInPlaylist = UserConfig.ShowPathInPlaylist;
            TrackTableViewModel.Instance.ShowPathInLibrary = UserConfig.ShowPathInLibrary;

            plsel = new PlaylistSelector(Playlist.DataContext as PlaylistViewModel);
            if (plsel.SelectedDefinition != null)
                PlaylistSerializer.Load(Playlist.DataContext as PlaylistViewModel, plsel.SelectedDefinition.Path);

            mainDispatcher = TableView.AlbumView.Dispatcher;
            TableView.AlbumView.SetMediaLibrary(Library.Current);
            TableView.ArtistView.SetMediaLibrary(Library.Current);
            TableView.TrackView.SetMediaLibrary(Library.Current);
            TrackTable.SetMediaLibrary(Library.Current);
            SmartPlayer.PlayListVM = Playlist.DataContext as ViewModels.PlaylistViewModel;

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

            Width += 1;

            StateChanged += MainWindow_StateChanged;

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
        }

        private void Query_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Library.Current.ClearSelection();
            Library.Current._query = _query;
            Library.Current.QueryMe(ViewMode);
            query_timer.Stop();
        }

        public void ResetFilter()
        {
            query_timer.Start();
        }

        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FilterBox.Text == FilterHint) return;

            _query = (sender as TextBox).Text;
            query_timer.Stop();
            query_timer.Start();
        }

        private void _This_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) ctrl_down = true;

            else if (e.Key == Key.F12)
            {
                if (WindowStyle != WindowStyle.None)
                {
                    WindowStyle = WindowStyle.None;
                }
                else
                {
                    WindowStyle = WindowStyle.ThreeDBorderWindow;
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
                plsel = new PlaylistSelector(Playlist.DataContext as PlaylistViewModel);
                var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Playlists");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                if (plsel.SelectedDefinition == null)
                {
                    var def = new PlaylistDefinition()
                    {
                        AutoSelect = true,
                        Name = "autosave",
                        Path = Path.Combine(path, "autosave.pls")
                    };

                    var auto = plsel.PlaylistDefs.FirstOrDefault(_def => _def.Name == def.Name);
                    if (auto != null)
                    {
                        plsel.PlaylistDefs.ForEach(d => d.AutoSelect = false);
                        auto.AutoSelect = true;
                        plsel.SelectedDefinition = auto;
                    }
                    else
                    {
                        plsel.PlaylistDefs.Add(def);
                        plsel.SelectedDefinition = def;
                    }
                    plsel.SavePlaylistXML();
                }
                PlaylistSerializer.Serialize(plsel.SelectedDefinition.Path, (Playlist.DataContext as PlaylistViewModel).Tracks);
            }

            SmartPlayer.Shutdown();
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

        public PlaylistInfo[] GetSelectedTracks()
        {
            switch (MainViews.SelectedIndex)
            {
                case 0: return null;
                case 1: return AlbumDetails.GetSelectedTracks();
                case 2: return TrackTable.GetSelected().ToArray();
                case 3: return Favourites.GetSelected();
                default: return null;
            }
        }

        string getCountString(int count)
        {
            string cnt = count.ToString();
            int cnt_len = cnt.Length;
            for (int i = 0; i < 9 - cnt_len; i++)
                cnt = "0" + cnt;
            return cnt;
        }

        private void CreateSummyary_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new PlaylistCover(PlaylistViewModel.Instance.Tracks);
            dialog.Topmost = true;
            dialog.ShowDialog();
        }

        private void ExportPlaylistInformation_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            foreach (PlaylistInfo pi in (Playlist.DataContext as PlaylistViewModel).Tracks)
            {
                sb.Append(pi.TrackNumber.Trim() + " " + pi.Duration + " " + pi.Artists + " - " + pi.Title + " - " + pi.Album);
                long year = 0;
                if (long.TryParse(pi.Year, out year) && year != 0)
                    sb.Append(" (" + pi.Year.ToString() + ")");
                sb.Append(Environment.NewLine);
            }
            Clipboard.SetText(sb.ToString());
            MessageBox.Show("Information has been copied to the clipboard.");
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
                foreach (PlaylistInfo pi in (Playlist.DataContext as PlaylistViewModel).Tracks)
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

            BringIntoView();
        }

        private void _This_Loaded(object sender, RoutedEventArgs e)
        {
            mainDispatcher = TableView.AlbumView.Dispatcher;
            Width += 1;
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

        private void MinimizeWindow(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MaximizeWindow(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;

        private void btnEqualizer_MouseDown(object sender, MouseButtonEventArgs e)
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

        private void btnSmallView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double windowHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = SystemParameters.PrimaryScreenWidth;

            windowHeight *= 0.85;
            windowWidth *= 0.305;

            WindowState = WindowState.Normal;
            Width = windowWidth;
            Height = windowHeight;
            cdLibrary.Width = new GridLength(1);

            Left = SystemParameters.PrimaryScreenWidth - windowWidth - 20;
            Top = (SystemParameters.PrimaryScreenHeight - windowHeight) / 2;
        }

        private void btFullView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Normal;
            Width = System.Windows.SystemParameters.PrimaryScreenWidth * 0.75;
            Height = 850;
            cdLibrary.Width = new GridLength(60, GridUnitType.Star);
            cdPlaylist.Width = new GridLength(25, GridUnitType.Star);

            Left = (SystemParameters.PrimaryScreenWidth / 2) - (ActualWidth / 2);
            Top = (SystemParameters.PrimaryScreenHeight / 2) - (ActualHeight / 2);
        }

        private void btnClearFavorites_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Library.Instance.connector.ClearFavorites();
        }

        private void FilterBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (FilterBox.Text == FilterHint)
            {
                FilterBox.TextChanged -= FilterBox_TextChanged;
                FilterBox.Text = "";
                FilterBox.TextChanged += FilterBox_TextChanged;
            }
            else
            {
                e.Handled = true;
                FilterBox.Focus();
                FilterBox.SelectAll();
            }
        }

        private void FilterBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (FilterBox.Text == "")
            {
                FilterBox.TextChanged -= FilterBox_TextChanged;
                FilterBox.Text = FilterHint;
                FilterBox.TextChanged += FilterBox_TextChanged;
            }
        }

        void ClearFocus()
        {
            UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
            if (elementWithFocus is System.Windows.Controls.TextBox tb)
            {
                if (Keyboard.FocusedElement != null)
                {
                    Keyboard.FocusedElement.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent));
                    Keyboard.ClearFocus();
                }
            }
        }

        private void FilterBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ClearFocus();
                query_timer.Stop();
                query_timer.Start();
            }
        }

        private void FilterBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            FilterBox.Focus();
            FilterBox.SelectAll();
        }

        private void btnMiniview_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pt = SmartPlayer.btnForward.TransformToAncestor(MainWindow.Instance).Transform(new Point(0, 0));
            WindowState = WindowState.Normal;
            Width = SmartPlayer.ActualWidth + 20;
            Height = pt.Y + SmartPlayer.btnForward.Height + 45;
            Top = SystemParameters.WorkArea.Height - Height;
            Left = SystemParameters.WorkArea.Width - Width - 10;
        }
    }
}

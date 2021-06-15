using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Timers;
using Classes;
using ViewModels;
using System.Windows.Threading;
using System.Linq;
using Interfaces;
using Plugins;
using Equalizer;
using Dialogs;
using Models;
using Mpfree4k.Enums;
using Configuration;
using MpFree4k;
using Layers;
using ViewModels;
using System.Windows.Media;

namespace Controls
{
    public class ValueChangedEvent : EventArgs
    {
        public string Key;
        public string Value;
    }

    public class ViewModelChangedEventArgs : EventArgs
    {
        public ViewModelChangedEventArgs(object info) { this.value = info; }
        public object value;
    }

    public partial class Player : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ValueChangedEvent> ValueChanged;
        public event EventHandler<ViewModelChangedEventArgs> ViewModelChanged;

        public static Player Instance;

        System.Timers.Timer CheckSongTimer;
        Dispatcher dispatcher = null;
        PlaylistInfo current_track = null;
        
        private PlaylistViewModel _playlistVM = null;
        private List<bool> PlayStates = new List<bool>();
        private IMediaPlugin MediaPlayer;
        int forward = 5;
        int muteVolumne = 50;
        double trackLength = 0;
        double pause_position = 0;

        RemainingMode remainingMode = RemainingMode.Elapsed;

        // deactivated
        // Thread tPlayReverse = new Thread(() => reverse());
        //bool stopreverse = false;

        Playmode playmode = Playmode.Play;

        public Player()
        {
            dispatcher = Dispatcher;

            InitializeComponent();

            Instance = this;

            btnMute.DataContext = this;

            Rebuild();

            CheckSongTimer = new System.Timers.Timer(300);
            CheckSongTimer.AutoReset = true;
            CheckSongTimer.Elapsed += CheckSongTimer_Elapsed;
            Loaded += Player_Loaded;
        }

        public PlaylistViewModel PlayListVM
        {
            get => _playlistVM;
            set
            {
                if (_playlistVM != null) _playlistVM.PropertyChanged -= _playlistVM_PropertyChanged;
                _playlistVM = value;
                _playlistVM.PropertyChanged += _playlistVM_PropertyChanged;
            }
        }

        public static ImageSource defaultImage = null;

        public double ButtonSize => (double)UserConfig.ControlSize;

        public bool SongEnded => MediaPlayer.SongEnded;

        public void Play(PlaylistInfo fileInfo, bool from_start = false)
        {
            if (fileInfo == null)
            {
                Stop();
                return;
            }

            ViewModelChanged?.Invoke(null, new ViewModelChangedEventArgs(fileInfo));

            bool paused = MediaPlayer.IsPaused;

            MediaPlayer.SongEnded = false;
            CheckSongTimer.Start();
            current_track = fileInfo;

            MediaPlayer.Stop();
            MediaPlayer.Position = 0;
            MediaPlayer.URL = fileInfo.Path;
            lblTrack.Content = fileInfo.Path;

            MediaPlayer.Init(fileInfo.Path);

            if (MediaPlayer.Duration == 0)
            {
                if (PlayListVM.RepeatMode == RepeatMode.GoThrough ||
                    PlayListVM.RepeatMode == RepeatMode.Loop ||
                    PlayListVM.RepeatMode == RepeatMode.Shuffle)
                {
                    Next();
                }
                return;
            }

            double duration = (int)MediaPlayer.Duration;
            InitPositionMarker((int)duration);
            lblTotalTrackDuration.Content = Utilities.LibraryUtils.GetDurationString((int)duration);
            setProgressSliderTicks(duration);
            ShowWindowTitleByFile(fileInfo);

            if (!MediaPlayer.IsPaused)
            {
                MediaPlayer.Stop();
                MediaPlayer.Position = 0;
            }

            try
            {
                if (!MediaPlayer.IsPaused)
                    MediaPlayer.Init(current_track.Path);

                if (playmode == Playmode.Play)
                {
                    MediaPlayer.Play();
                }
                else
                {
                    //Unplay(fileInfo);
                    return;
                }

                RememberTrack(current_track);

                if (paused && !from_start)
                    MediaPlayer.Position = pause_position;

                try
                {
                    DisplayTrackImage(fileInfo);
                }
                catch
                {
                    TrackImage.Source = null;
                    //TrackImageBig.Source = null;
                }

                duration = MediaPlayer.Duration;
                InitPositionMarker((int)duration);
                lblTotalTrackDuration.Content = Utilities.LibraryUtils.GetDurationString((int)duration);
                setProgressSliderTicks(duration);

                Raise("Play");

            }
            catch (Exception exc)
            {
                if (PlayListVM.RepeatMode != RepeatMode.Once ||
                    PlayListVM.RepeatMode != RepeatMode.RepeatOne)
                    Next();
            }
        }

        private bool _isCheckedState = false;
        public bool IsCheckedState
        {
            get => _isCheckedState;
            set { _isCheckedState = value; Raise(nameof(IsCheckedState)); }
        }

        private Visibility _touchButtonsVisibility = Visibility.Visible;
        public Visibility TouchButtonsVisibility
        {
            get => _touchButtonsVisibility;
            set
            {
                _touchButtonsVisibility = value;
                Raise(nameof(TouchButtonsVisibility));
            }
        }

        private void _playlistVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Play")
            {
                current_track = PlayListVM.GetCurrent();
                Play(current_track);
            }
            else if (e.PropertyName == "PlayFromStart")
            {
                current_track = PlayListVM.GetCurrent();
                Play(current_track, true);
            }
        }

        public void Raise(string info)
        {
            if (info == "ButtonSize" || info == "FontSize")
            {
                double h = ButtonSize + 36;
                Height = h + (6.5 * (Math.Max((int)UserConfig.FontSize - 3, 0)));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public void Rebuild()
        {
            if (MediaPlayer != null)
            {
                MediaPlayer.PlayStateChanged -= MediaPlayer_PlayStateChanged;
                MediaPlayer.Stop();

                if (MediaPlayer is CSCorePlugin cs)
                    SpectrumViewModel.Instance?.RemoveSpectrum();

                MediaPlayer.Dispose();
            }

            if (UserConfig.PluginType == PluginTypes.WMPLib)
                MediaPlayer = new WMPLibPlugin();
            else
                MediaPlayer = new CSCorePlugin();

            MediaPlayer.SetVolume((int)sldVolume.Value);
            MediaPlayer.PlayStateChanged += MediaPlayer_PlayStateChanged;
        }

        private void MediaPlayer_PlayStateChanged(object sender, EventArgs e)
        {
            if (sender is string property)
            {
                if (property == "Play" && MediaPlayer is SpectrumViewModel model)
                {
                    Dispatcher.Invoke(() =>
                    {
                        model.ViewPort = new System.Drawing.Size((int)Spectrum.ActualWidth, (int)Spectrum.ActualHeight);
                        model._control = Spectrum;
                        Spectrum.SetViewModel(model);
                        Spectrum.ApplyProperties();
                        model.CreateSpectrum();
                    });
                }
            }
        }

        private void Player_Loaded(object sender, RoutedEventArgs e)
        {
            lblTrack.Content = "...";
            lblArtist.Content = "...";
            lblAlbum.Text = "...";
            lblYear.Text = "-";

            Height = (85 - 34) + ButtonSize - 34;
            Raise("ButtonSize");
        }

        public void OnThemeChanged()
        {
            Spectrum.ControlBackground = Resources["SpectrumBackground"] as System.Windows.Media.SolidColorBrush;
            Spectrum.ApplyProperties();
        }

        void UpdatePositionMarker(double position)
        {
            sldTrackSlider.ValueChanged -= sldTrackSlider_ValueChanged;
            sldTrackSlider.Value = position;
            sldTrackSlider.ValueChanged += sldTrackSlider_ValueChanged;
        }

        void InitPositionMarker(int duration) => sldTrackSlider.Maximum = duration;

        void ShowWindowTitleByFile(PlaylistInfo item)
        {
            SetTrackInfo(item);

            string trackName = "";

            if (item != null)
            {
                item.Path.Substring(item.Path.LastIndexOf(@"\") + 1);

                FileViewInfo info = new FileViewInfo(item.Path);
                info.CreateFileHandle();

                if (info._Handle != null)
                    trackName = info.Mp3Fields.Title + " - " + info.Mp3Fields.AlbumArtists;
            }
            ShowWindowTitle(trackName);

        }

        void SetTrackInfo(PlaylistInfo itm)
        {
            lblTrack.Content = "";
            lblArtist.Content = "";
            lblAlbum.Text = "";
            lblYear.Text = "";

            if (itm == null)
                return;

            string title = itm.Title;
            if (string.IsNullOrWhiteSpace(title))
                title = itm.Path;

            lblTrack.Content = title;
            lblArtist.Content = itm.Artists;
            lblAlbum.Text = itm.Album;
            lblYear.Text = itm.Year;

            trackLength = Utilities.LibraryUtils.DurationStringToSeconds(itm.Duration);
        }

        void ShowWindowTitle(string songName)
        {
            string cstr = string.Format("{0} - MpFree4k - soulflick", songName);
            (Window.GetWindow(this) as MainWindow).Title = cstr;
        }

        //private void Unplay(PlaylistItem pItm)
        //{
        //    playmode = Playmode.Unplay;
        //    tPlayReverse = new Thread(() => reverse());
        //    tPlayReverse.Start();
        //}

        //static void reverse()
        //{
        //    double position = _singleton.MediaPlayer.Position;
        //    double end = position;

        //    _singleton.MediaPlayer.Play();
        //    while (!_singleton.stopreverse && position > 0.1)
        //    {
        //        Thread.Sleep(100);
        //        _singleton.MediaPlayer.Forward(-0.2);
        //        position = _singleton.MediaPlayer.Position;
        //        _singleton.Dispatcher.BeginInvoke(new Action(() => _singleton.UpdatePositionMarker(position)));
        //    }

        //    _singleton.MediaPlayer.Position = end - 1;
        //    return;
        //}

        
        void RememberTrack(PlaylistInfo t)
        {
            Library.Instance.connector.SetTrack(t.Path);
            string[] tracks = Library.Instance.Current.Files.Where(f => f.Mp3Fields.Year.ToString() == t.Year && f.Mp3Fields.Album == t.Album).Select(y => y.Path).ToArray();
            Library.Instance.connector.SetAlbum(t.Album, t.Year.ToString(), tracks);
        }

        void CheckSong()
        {
            double progress = MediaPlayer.Position;
            string posStr = Utilities.LibraryUtils.GetDurationString((int)MediaPlayer.Position);

            string posStrRemaining = Utilities.LibraryUtils.SecondsToDuration(trackLength - progress);
            this.dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                lblProgress.Content = (String.IsNullOrEmpty(posStr)) ? "00:00" : (remainingMode == RemainingMode.Elapsed) ? posStr : posStrRemaining;
                UpdatePositionMarker(progress);
            }));


            RepeatMode repeatMode = PlayListVM.RepeatMode;
            if (SongEnded)
            {
                MediaPlayer.SongEnded = false;
                if (repeatMode == RepeatMode.RepeatOne)
                    Play(current_track);
                else if (repeatMode == RepeatMode.Once)
                    Stop();
                else if (repeatMode == RepeatMode.GoThrough)
                    Next();
                else if (repeatMode == RepeatMode.Loop)
                    Next();
                else if (repeatMode == RepeatMode.Shuffle)
                    Shuffle();
            }
        }

        void setProgressSliderTicks(double duration)
        {
            sldTrackSlider.TickFrequency = 10; // (int)(duration / 30);
            System.Windows.Media.DoubleCollection dbls = new System.Windows.Media.DoubleCollection();
            for (int i = 0; i <= (int)(duration / 30); i++)
                dbls.Add(i * 30);
            sldTrackSlider.Ticks = dbls;
        }

        void DisplayTrackImage(PlaylistInfo itm)
        {
            if (itm.Image != null)
            {
                TrackImage.Source = itm.Image;
                //(Window.GetWindow(this) as MainWindow).Player.TrackImageBig.Source = itm.Image;
                return;
            }

            try
            {
                TagLib.File _tmp = TagLib.File.Create(itm.Path);
                TrackImage.Source = TagLibConvertPicture.GetImageFromTag(_tmp.Tag.Pictures);
                //(Window.GetWindow(this) as MainWindow).Player.TrackImageBig.Source = TrackImage.Source;
            }
            catch
            {
                TrackImage.Source = null;
                //(Window.GetWindow(this) as MainWindow).Player.TrackImageBig.Source = null;
            }

            if (TrackImage.Source == null)
            {
                TrackImage.Source = DefaultImage;
            }
        }
       
        public void Pause()
        {
            if (MediaPlayer.IsPaused)
            {
                Play(current_track);
            }
            else
            {
                pause_position = MediaPlayer.Position;
                CheckSongTimer.Stop();
                MediaPlayer.Pause();
            }
        }

        public void Shutdown()
        {
            Stop();
            MediaPlayer.Stop();
            MediaPlayer.Dispose();
            MediaPlayer = null;
        }

        public void Stop()
        {
            //tPlayReverse.Abort();

            for (int i = 0; i < PlayStates.Count; i++)
                PlayStates[i] = false;

            MediaPlayer.Stop();

            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                sldTrackSlider.Value = 0;
                lblProgress.Content = "00:00";
            }));
            CheckSongTimer.Stop();
        }

        void Next()
        {
            current_track = PlayListVM.GetNext();
            dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                Play(current_track);
                ShowWindowTitleByFile(current_track);
            }));

        }

        void Shuffle()
        {
            dispatcher.BeginInvoke(new Action(() =>
            {
                current_track = PlayListVM.GetShuffle();
                ShowWindowTitleByFile(current_track);
                Play(current_track);
            }));

        }

        void Previous()
        {
            current_track = PlayListVM.GetPrevious();
            ShowWindowTitleByFile(current_track);
            if (MediaPlayer.IsPlaying)
                Play(current_track);
        }

        public ImageSource DefaultImage  
        {
            get
            {
                if (defaultImage == null)
                    defaultImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(@"pack://application:,,,/" +
                        System.Reflection.Assembly.GetCallingAssembly().GetName().Name + ";component/Images/no_album_cover.jpg", UriKind.Absolute));

                return defaultImage;
            }
        }

        private void CheckSongTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckSong();
            ValueChanged(null, new ValueChangedEvent() { Key = "PlayTimeUpdate", Value = MediaPlayer.Position.ToString() });
        }

        private void sldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => MediaPlayer?.SetVolume((int)sldVolume.Value);

        private void sldTrackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MediaPlayer == null) return;
            if (!MediaPlayer.HasMedia) return;

            MediaPlayer.Position = sldTrackSlider.Value;
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            Previous();
        }

        private void btnRewind_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.Position > 0)
                MediaPlayer.Position -= forward;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (playmode == Playmode.Unplay)
            {
                //tPlayReverse.Abort();
                MediaPlayer.Play();
                playmode = Playmode.Play;
                return;
            }
            playmode = Playmode.Play;
            current_track = PlayListVM.GetCurrent();
            Play(current_track);
        }

        private void btnReverse_Click(object sender, RoutedEventArgs e)
        {
            current_track = PlayListVM.GetCurrent();
            //Unplay(current_track);
        }

        private void btnPause_Click(object sender, RoutedEventArgs e) => Pause();

        private void btnStop_Click(object sender, RoutedEventArgs e) => Stop();

        private void btnForward_Click(object sender, RoutedEventArgs e) => MediaPlayer.Position += forward;

        private void btnNext_Click(object sender, RoutedEventArgs e) => Next();

        private void btnMute_Checked(object sender, RoutedEventArgs e)
        {
            muteVolumne = (int)sldVolume.Value;
            MediaPlayer.SetVolume(0);
            sldVolume.Value = 0;
            IsCheckedState = btnMute.IsChecked == true;
        }

        private void btnMute_Unchecked(object sender, RoutedEventArgs e)
        {
            MediaPlayer.SetVolume(muteVolumne);
            sldVolume.Value = muteVolumne;
            IsCheckedState = btnMute.IsChecked == true;
        }

        private void sldTrackSlider_StylusMove(object sender, System.Windows.Input.StylusEventArgs e) => MediaPlayer.SetVolume(0);

        private void sldTrackSlider_StylusDown(object sender, System.Windows.Input.StylusDownEventArgs e) => MediaPlayer.SetVolume(muteVolumne);

        private void lblProgress_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            remainingMode = remainingMode == RemainingMode.Elapsed ? RemainingMode.Remaining : RemainingMode.Elapsed;

        private void Spectrum_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Spectrum.GraphType == GraphType.Bar)
            {
                Spectrum.GraphType = GraphType.Line;
                Spectrum.BarCount = 100;
            }
            else if (Spectrum.GraphType == GraphType.Line)
            {
                Spectrum.GraphType = GraphType.Band;
                Spectrum.BarCount = 120;
            }
            else if (Spectrum.GraphType == GraphType.Band)
            {
                Spectrum.GraphType = GraphType.ThinBand;
                Spectrum.BarCount = 256;
            }
            else if (Spectrum.GraphType == GraphType.ThinBand)
            {
                Spectrum.GraphType = GraphType.ThinBandWithTips;
                Spectrum.BarCount = 256;
            }
            else if (Spectrum.GraphType == GraphType.ThinBandWithTips)
            {
                Spectrum.GraphType = GraphType.ThinLine;
                Spectrum.BarCount = 512;
            }
            else if (Spectrum.GraphType == GraphType.ThinLine)
            { 
                Spectrum.GraphType = GraphType.Bar;
                Spectrum.BarCount = 32;
            }

            Spectrum.ApplyProperties();
            Spectrum.Redraw();
        }

        private void btnPlayNow_Click(object sender, RoutedEventArgs e)
        {
            var tracks = MainWindow.Instance.GetSelectedTracks();
            if (tracks != null && tracks.Any())
                PlaylistViewModel.Play(tracks);
        }

        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            var tracks = MainWindow.Instance.GetSelectedTracks();
            if (tracks != null && tracks.Any())
                PlaylistViewModel.Insert(tracks);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var tracks = MainWindow.Instance.GetSelectedTracks();
            if (tracks != null && tracks.Any())
                PlaylistViewModel.Add(tracks);
        }

        private void TrackImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var current = PlaylistViewModel.Instance.CurrentSong;
            if (current == null)
                return;

            var _f = new FileViewInfo(current.Path);
            var info = new TrackInfo(_f, Library.Instance.Current.Files);
            info.ShowDialog();
        }
    }
}

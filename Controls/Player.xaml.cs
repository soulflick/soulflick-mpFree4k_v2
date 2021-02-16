using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using MpFree4k.Classes;
using System.Timers;
using Classes;
using MpFree4k.ViewModels;
using System.Windows.Threading;
using MpFree4k.Enums;
using MpFree4k.Layers;
using System.Linq;
using System.Threading;
using MpFree4k.Interfaces;
using MpFree4k.Plugins;
using WPFEqualizer;

namespace MpFree4k.Controls
{
    public class ValueChangedEvent : EventArgs
    {
        public string Key;
        public string Value;
    }

    public enum RemainingMode
    {
        Elapsed,
        Remaining
    }

    public enum Playmode
    {
        Play,
        Unplay
    }

    public partial class Player : UserControl, INotifyPropertyChanged
    {
        private PlaylistViewModel _playlistVM = null;
        private int forward = 5;
        public PlaylistViewModel PlayListVM
        {
            get => _playlistVM;
            set
            {
                if (_playlistVM != null)
                    _playlistVM.PropertyChanged -= _playlistVM_PropertyChanged;
                _playlistVM = value;
                _playlistVM.PropertyChanged += _playlistVM_PropertyChanged;
            }
        }

        public double ButtonSize
        {
            get => (double)UserConfig.ControlSize;
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

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };
        public event EventHandler<ValueChangedEvent> ValueChanged;

        public void NotifyPropertyChanged(String info)
        {
            if (info == "ButtonSize" || info == "FontSize")
            {
                double h = ButtonSize + 36;
                this.Height = h + (6.5 * (Math.Max((int)UserConfig.FontSize - 3, 0)));
            }

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private bool _isCheckedState = false;
        public bool IsCheckedState
        {
            get => _isCheckedState;
            set { _isCheckedState = value; NotifyPropertyChanged("IsCheckedState"); }
        }


        private System.Timers.Timer CheckSongTimer;
        public bool SongEnded { get => MediaPlayer.SongEnded; }

        bool manualPlayStateChanged = false;

        int muteVolumne = 50;
        List<bool> PlayStates = new List<bool>();

        Dispatcher dispatcher = null;
        private static Player _singleton = null;

        IMediaPlugin MediaPlayer;
        public Player()
        {
            dispatcher = this.Dispatcher;

            InitializeComponent();

            btnMute.DataContext = this;

            Rebuild();

            CheckSongTimer = new System.Timers.Timer(300);
            CheckSongTimer.AutoReset = true;
            CheckSongTimer.Elapsed += CheckSongTimer_Elapsed;
            Loaded += Player_Loaded;

            _singleton = this;
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
            if (sender is String property)
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

            this.Height = (85 - 34) + ButtonSize - 34;
            NotifyPropertyChanged("ButtonSize");
        }

        public void OnThemeChanged()
        {
            Spectrum.ControlBackground = this.Resources["SpectrumBackground"] as System.Windows.Media.SolidColorBrush;
            Spectrum.ApplyProperties();
        }

        private void CheckSongTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckSong();
            ValueChanged(null, new ValueChangedEvent() { Key = "PlayTimeUpdate", Value = MediaPlayer.Position.ToString() });
        }


        private void sldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaPlayer?.SetVolume((int)sldVolume.Value);
        }

        void UpdatePositionMarker(double position)
        {
            sldTrackSlider.ValueChanged -= sldTrackSlider_ValueChanged;
            sldTrackSlider.Value = position;
            sldTrackSlider.ValueChanged += sldTrackSlider_ValueChanged;
        }

        void InitPositionMarker(int duration)
        {
            sldTrackSlider.Maximum = duration;
        }

        void ShowWindowTitleByFile(PlaylistItem item)
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

        private Visibility _touchButtonsVisibility = Visibility.Visible;
        public Visibility TouchButtonsVisibility
        {
            get => _touchButtonsVisibility;
            set
            {
                _touchButtonsVisibility = value;
                NotifyPropertyChanged(nameof(TouchButtonsVisibility));
            }
        }

        void SetTrackInfo(PlaylistItem itm)
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

            trackLength = DurationStringToSeconds(itm.Duration);
        }

        double DurationStringToSeconds(string duration)
        {
            double secs = 0;
            int idx = duration.IndexOf(':');

            if (idx <= 0)
            {
                Double.TryParse(duration, out secs);
                return secs;
            }
            int idx2;
            string s_hrs, s_mins, s_secs;
            if (duration.Count(x => x == ':') > 1)
            {

                idx2 = duration.LastIndexOf(':');
                s_hrs = duration.Substring(0, idx);
                s_mins = duration.Substring(idx + 1, idx2 - idx - 1);
                s_secs = duration.Substring(idx2 + 1);
            }
            else
            {
                s_hrs = "0";
                s_mins = duration.Substring(0, idx);
                s_secs = duration.Substring(idx + 1);
            }

            if (string.IsNullOrEmpty(s_mins))
                s_mins = "0";

            if (string.IsNullOrEmpty(s_secs))
                s_secs = "0";

            double d_hrs = Convert.ToDouble(s_hrs) * 3600;
            double d_mins = Convert.ToDouble(s_mins) * 60;
            double d_secs = Convert.ToDouble(s_secs);

            return d_hrs + d_mins + d_secs;
        }

        string secondsToDuration(double secs)
        {
            double d_hrs = Math.Floor(secs / 3600);
            double d_mins = Math.Floor((secs - (d_hrs * 3600)) / 60);
            double d_seconds = Math.Floor(secs - (d_hrs * 3600) - (d_mins * 60));

            string dur = "";
            if (d_hrs > 0)
                dur += d_hrs.ToString() + ":";
            if (d_mins < 10)
                dur += "0";
            dur += d_mins.ToString() + ":";
            if (d_seconds < 10)
                dur += "0";
            dur += d_seconds.ToString();

            return dur;
        }

        double trackLength = 0;

        void ShowWindowTitle(string songName)
        {
            string cstr = string.Format("{0} - MpFree4k - soulflick", songName);
            (Window.GetWindow(this) as MainWindow).Title = cstr;
        }

        Thread tPlayReverse = new Thread(() => reverse());
        Playmode playmode = Playmode.Play;
        private void Unplay(PlaylistItem pItm)
        {
            playmode = Playmode.Unplay;
            tPlayReverse = new Thread(() => reverse());
            tPlayReverse.Start();
        }

        bool stopreverse = false;
        static void reverse()
        {
            double position = _singleton.MediaPlayer.Position;
            double end = position;

            _singleton.MediaPlayer.Play();
            while (!_singleton.stopreverse && position > 0.1)
            {
                Thread.Sleep(100);
                _singleton.MediaPlayer.Forward(-0.2);
                position = _singleton.MediaPlayer.Position;
                _singleton.Dispatcher.BeginInvoke(new Action(() => _singleton.UpdatePositionMarker(position)));
            }

            _singleton.MediaPlayer.Position = end - 1;
            return;
        }


        PlaylistItem current_track = null;
        public void Play(PlaylistItem fileInfo, bool from_start = false)
        {
            if (fileInfo == null)
            {
                Stop();
                return;
            }

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
                    return;
                }
                return;
            }


            double duration = (int)MediaPlayer.Duration;
            InitPositionMarker((int)duration);
            lblTotalTrackDuration.Content = GetDurationString((int)duration);
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
                    Unplay(fileInfo);
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
                lblTotalTrackDuration.Content = GetDurationString((int)duration);
                setProgressSliderTicks(duration);

                NotifyPropertyChanged("Play");

            }
            catch (Exception exc)
            {
                if (PlayListVM.RepeatMode != RepeatMode.Once ||
                    PlayListVM.RepeatMode != RepeatMode.RepeatOne)
                    Next();
            }
        }

        void RememberTrack(PlaylistItem t)
        {
            Library._singleton.connector.SetTrack(t.Path);
            string[] tracks = Library._singleton.Current.Files.Where(f => f.Mp3Fields.Year.ToString() == t.Year && f.Mp3Fields.Album == t.Album).Select(y => y.Path).ToArray();
            Library._singleton.connector.SetAlbum(t.Album, t.Year.ToString(), tracks);
        }

        void CheckSong()
        {
            double progress = MediaPlayer.Position;
            string posStr = GetDurationString((int)MediaPlayer.Position);

            string posStrRemaining = secondsToDuration(trackLength - progress);
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

        void DisplayTrackImage(PlaylistItem itm)
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
                showDefaultPicture();
            }
        }

        public static string GetDurationString(int duration)
        {
            int seconds, minutes, hours;
            hours = duration / 3600;
            minutes = (duration - (hours * 3600)) / 60;
            seconds = duration - (hours * 3600) - minutes * 60;

            string ret = "";
            if (hours > 0) ret = hours.ToString() + ":";
            if (minutes < 10) ret += "0";
            ret += minutes.ToString() + ":";
            if (seconds < 10) ret += "0";
            ret += seconds.ToString();

            return ret;
        }

        double pause_position = 0;
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
            tPlayReverse.Abort();

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

        private void btnRewind_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.Position > 0)
                MediaPlayer.Position -= forward;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (playmode == Playmode.Unplay)
            {
                tPlayReverse.Abort();
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
            Unplay(current_track);
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Position += forward;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            Next();
        }

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

        public void showDefaultPicture()
        {
            TrackImage.Source = new System.Windows.Media.Imaging.BitmapImage(
new System.Uri(@"pack://application:,,,/" +
System.Reflection.Assembly.GetCallingAssembly().GetName().Name +
";component/" + "Images/no_album_cover.jpg", System.UriKind.Absolute));

            //(Window.GetWindow(this) as MainWindow).Player.TrackImageBig.Source = TrackImage.Source;
        }

        private void sldTrackSlider_StylusMove(object sender, System.Windows.Input.StylusEventArgs e)
        {
            MediaPlayer.SetVolume(0);
        }

        private void sldTrackSlider_StylusDown(object sender, System.Windows.Input.StylusDownEventArgs e)
        {
            MediaPlayer.SetVolume(muteVolumne);
        }

        private RemainingMode remainingMode = RemainingMode.Elapsed;
        private void lblProgress_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            remainingMode = remainingMode == RemainingMode.Elapsed ? RemainingMode.Remaining : RemainingMode.Elapsed;
        }

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
    }
}

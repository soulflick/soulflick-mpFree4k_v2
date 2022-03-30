using Extensions;
using Controls;
using System;
using Models;
using System.Collections.Generic;
using Interfaces;
using Mpfree4k.Enums;
using Plugins;
using Equalizer;
using Configuration;
using Layers;
using System.Linq;

namespace ViewModels
{
    public class PlayerViewModel : Notify
    {
        IPlayer IPlayer;

        public PlayerViewModel(IPlayer Player)
        {
            IPlayer = Player;
        }

        private Models.PlaylistInfo _current;
        public  Models.PlaylistInfo  current
        {
            get => _current;
            set => Update(ref _current, value, nameof(current));
        }

        public static PlayerViewModel Instance = null;

        public PlayerViewModel()
        {
            Instance = this;
            Rebuild();
            IPlayer.ViewModelChanged += (s, e) => OnDisplay(e);
        }

        private void OnDisplay(ViewModelChangedEventArgs e) => current = (Models.PlaylistInfo)e.value;
        public event EventHandler<ValueChangedEvent> ValueChanged;

        public PlaylistInfo current_track = null;
        public List<bool> PlayStates = new List<bool>();
        public IMediaPlugin MediaPlayer;
        public int forward = 5;
        public int muteVolumne = 50;
        public double trackLength = 0;
        public double pause_position = 0;

        public RemainingMode remainingMode = RemainingMode.Elapsed;

        // deactivated
        // Thread tPlayReverse = new Thread(() => reverse());
        //bool stopreverse = false;

        public Playmode playmode = Playmode.Play;

        public void Rebuild()
        {
            if (MediaPlayer != null)
            {
                MediaPlayer.PlayStateChanged -= IPlayer.MediaPlayer_PlayStateChanged;
                MediaPlayer.Stop();

                if (MediaPlayer is CSCorePlugin cs)
                    SpectrumViewModel.Instance?.RemoveSpectrum();

                MediaPlayer.Dispose();
            }

            if (UserConfig.PluginType == PluginTypes.WMPLib)
                MediaPlayer = new WMPLibPlugin();
            else
                MediaPlayer = new CSCorePlugin();

            MediaPlayer.SetVolume(IPlayer.Volume);
            MediaPlayer.PlayStateChanged += IPlayer.MediaPlayer_PlayStateChanged;
        }

        public void RememberTrack(PlaylistInfo t)
        {
            Library.Instance.connector.SetTrack(t.Path);
            string[] tracks = Library.Instance.Current.Files.Where(f => f.Mp3Fields.Year.ToString() == t.Year && f.Mp3Fields.Album == t.Album).Select(y => y.Path).ToArray();
            Library.Instance.connector.SetAlbum(t.Album, t.Year.ToString(), tracks);
        }

        public void CheckSong()
        {
            double progress = MediaPlayer.Position;
            string posStr = Utilities.LibraryUtils.GetDurationString((int)MediaPlayer.Position);

            string posStrRemaining = Utilities.LibraryUtils.SecondsToDuration(trackLength - progress);
            IPlayer.dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                IPlayer.updateProgress((String.IsNullOrEmpty(posStr)) ? "00:00" : (remainingMode == RemainingMode.Elapsed) ? posStr : posStrRemaining, progress);
            }));


            RepeatMode repeatMode = IPlayer.PlayListVM.RepeatMode;
            if (MediaPlayer.SongEnded)
            {
                MediaPlayer.SongEnded = false;
                if (repeatMode == RepeatMode.RepeatOne)
                    IPlayer.Play(current_track);
                else if (repeatMode == RepeatMode.Once)
                    IPlayer.Stop();
                else if (repeatMode == RepeatMode.GoThrough)
                    IPlayer.Next();
                else if (repeatMode == RepeatMode.Loop)
                    IPlayer.Next();
                else if (repeatMode == RepeatMode.Shuffle)
                    IPlayer.Shuffle();
            }
        }

        public void onStart()
        {
            if (playmode == Playmode.Unplay)
            {
                //tPlayReverse.Abort();
                MediaPlayer.Play();
                playmode = Playmode.Play;
                return;
            }
            playmode = Playmode.Play;
            current_track = IPlayer.PlayListVM.GetCurrent();
            IPlayer.Play(current_track);
        }
    }
}

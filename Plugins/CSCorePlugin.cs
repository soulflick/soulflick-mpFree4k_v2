using CSCore;
using CSCore.SoundOut;
using Interfaces;
using System;
using Equalizer;

namespace Plugins
{
    public class CSCorePlugin : SpectrumViewModel, IMediaPlugin
    {
        private IWaveSource MediaPlayer = null;

        public event EventHandler ProgressChanged;
        public event EventHandler PlayStateChanged;

        private DateTime StartSongTime;
        private System.Timers.Timer CheckSongTimer;
        private System.Timers.Timer GoToNextSongTimer;

        public CSCorePlugin() : base(null)
        {
            CheckSongTimer = new System.Timers.Timer(100);
            CheckSongTimer.Elapsed += CheckSongTimer_Elapsed;

            GoToNextSongTimer = new System.Timers.Timer(200);
            GoToNextSongTimer.Elapsed += GoToNextSongTimer_Elapsed;
        }

        private double logicPosition = 0;
        DateTime startTime;
        private void GoToNextSongTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            logicPosition += (DateTime.Now - startTime).TotalMilliseconds / 1000;
            if (logicPosition > Duration)
            {
                GoToNextSongTimer.Stop();
                GoToNextSongTimer.Enabled = false;
                GoToNextSongTimer.Elapsed -= GoToNextSongTimer_Elapsed;
                logicPosition = 0;
                SongEnded = true;
            }
        }

        private void CheckSongTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan songPlayed = DateTime.Now - StartSongTime;
            if (Position >= Duration - 2 || songPlayed.TotalSeconds > 3 && Position < 1)
            {
                CheckSongTimer.Stop();

                logicPosition = Position;
                startTime = DateTime.Now;

                GoToNextSongTimer.Elapsed -= GoToNextSongTimer_Elapsed;
                GoToNextSongTimer.Elapsed += GoToNextSongTimer_Elapsed;
                GoToNextSongTimer.Enabled = true;
                GoToNextSongTimer.Start();
            }
        }

        private double _duration = 0;
        public double Duration
        {
            get => _duration;
            set
            {
                _duration = value;
            }
        }

        private double _position = 0;
        public double Position
        {
            get
            {
                if (IsPlaying)
                {
                    _position = sampleSource?.GetPosition().TotalMilliseconds / 1000 ?? _position;
                }
                return _position;
            }
            set
            {
                _position = value;
                TimeSpan pos = new TimeSpan(0, 0, 0, Math.Max(0, (int)value), Math.Max(0, (int)(value % 1) * 1000));
                try
                {
                    sampleSource?.SetPosition(pos);
                }
                catch { }
                
            }
        }

        public void Forward(int msecs)
        {
            _source.Position += msecs;
        }

        public double Volume { get; set; }

        private bool _songEnded = false;
        public bool SongEnded
        {
            get => _songEnded;
            set => _songEnded = value;
        }

        private string url;
        public string URL
        {
            get => url;
            set => url = value;
        }

        public void Init(string url)
        {
            SongEnded = false;

            MediaFile = url;

            Init();

            MediaPlayer = source;
            Duration = sampleSource?.GetLength().TotalMilliseconds / 1000 ?? 0;
            SetVolume(_volume);
            _position = 0;
        }

        private int _volume = 50;
        public void SetVolume(int value)
        {
            value = Math.Max(Math.Min(value, 100), 0);
            if (_soundOut != null)
            {
                _volume = value;
                _soundOut.Volume = (float)value / 100;
            }
        }

        public bool IsPlaying => _soundOut?.PlaybackState == PlaybackState.Playing;

        public bool IsPaused => _soundOut?.PlaybackState == PlaybackState.Paused;

        public bool HasMedia => Duration > 0;

        public void Forward(double msecs)
        {
            if (_soundOut != null)
                _soundOut.WaveSource.Position += (long)msecs;
        }

        public void Backward(double msecs)
        {
            if (_soundOut != null)
                _soundOut.WaveSource.Position -= (long)msecs;
        }

        public void Pause() => _soundOut?.Pause();

        public void Play()
        {
            SongEnded = false;
            GoToNextSongTimer.Stop();
            GoToNextSongTimer.Enabled = false;
            GoToNextSongTimer.Elapsed -= GoToNextSongTimer_Elapsed;
            logicPosition = 0;

            _soundOut.Play();

            PlayStateChanged?.Invoke("Play", EventArgs.Empty);
            StartSongTime = DateTime.Now;
            CheckSongTimer.Start();
        }

        public void Stop()
        {
            _soundOut?.Stop();
            _position = 0;
        }

        public void Dispose()
        {
            _soundOut?.Dispose();
            MediaPlayer?.Dispose();
        }
    }
}

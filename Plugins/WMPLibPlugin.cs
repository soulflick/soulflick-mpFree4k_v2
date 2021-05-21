using Interfaces;
using System;
using WMPLib;

namespace Plugins
{
    public class WMPLibPlugin : IMediaPlugin
    {
        private WMPLib.WindowsMediaPlayer MediaPlayer = null;
        public event EventHandler ProgressChanged;
        public event EventHandler PlayStateChanged;

        public WMPLibPlugin()
        {
            MediaPlayer = new WMPLib.WindowsMediaPlayer();
            MediaPlayer.settings.autoStart = true;

            this.MediaPlayer.PlayStateChange +=
               new WMPLib._WMPOCXEvents_PlayStateChangeEventHandler(wplayer_PlayStateChange);
        }

        public double Duration { get; set; }
        public double Position { 
            get => MediaPlayer.controls.currentPosition;
            set => MediaPlayer.controls.currentPosition = value;
        }

        public void Forward(int msecs) => MediaPlayer.controls.currentPosition += msecs;

        public double Volume { get; set; }
        public bool SongEnded { get; set; }

        private string url;
        public string URL
        {
            get => url;
            set => url = MediaPlayer.URL = value;
        }

        IWMPMedia currentMedia = null;
        public void Init(string url)
        {
            URL = url;
            currentMedia = MediaPlayer.newMedia(url);
            Duration = currentMedia.duration;
        }

        public void SetVolume(int value)
        {
            value = Math.Min(value, 100);
            MediaPlayer.settings.volume = value;
        }

        public bool IsPlaying => MediaPlayer.playState == WMPPlayState.wmppsPlaying;

        public bool IsPaused => MediaPlayer.playState == WMPPlayState.wmppsPaused;

        public bool HasMedia => currentMedia != null;

        public void Forward(double msecs) => MediaPlayer.controls.currentPosition += msecs;

        public void Backward(double msecs) => MediaPlayer.controls.currentPosition -= msecs;

        public void Pause() => MediaPlayer.controls.pause();

        public void Play() => MediaPlayer.controls.play();

        public void Stop() => MediaPlayer.controls.stop();

        void wplayer_PlayStateChange(int NewState)
        {
            switch ((WMPPlayState)NewState)
            {
                case WMPPlayState.wmppsUndefined:
                    SongEnded = true;
                    break;

                case WMPLib.WMPPlayState.wmppsMediaEnded:
                    SongEnded = true;
                    break;

                case WMPLib.WMPPlayState.wmppsPlaying:
                    SongEnded = false;
                    break;

                default:
                    break;
            }
        }

        public void Dispose()
        {
            currentMedia = null;
            MediaPlayer.close();
        }
    }
}

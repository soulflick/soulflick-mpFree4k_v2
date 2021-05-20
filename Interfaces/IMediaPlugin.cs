using System;

namespace Interfaces
{
    public interface IMediaPlugin : IDisposable
    {
        bool IsPlaying { get; }
        bool IsPaused { get; }
        bool HasMedia { get; }
        string URL { get; set; }
        double Duration { get; set; }
        double Position { get; set; }
        double Volume { get; set; }
        bool SongEnded { get; set; }
        void Init(string url);
        void Play();
        void Stop();
        void Pause();
        void Forward(double msecs);
        void Backward(double msecs);
        void SetVolume(int value);
        
        event EventHandler ProgressChanged;
        event EventHandler PlayStateChanged;
    }
}

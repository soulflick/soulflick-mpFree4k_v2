using Models;
using System;
using System.Windows.Threading;
using ViewModels;

namespace Controls
{
    public interface IPlayer
    {
        void MediaPlayer_PlayStateChanged(object sender, EventArgs e);
        event EventHandler<ViewModelChangedEventArgs> ViewModelChanged;
        int Volume { get; }

        Dispatcher dispatcher { get; }
        PlaylistViewModel PlayListVM { get; set; }
        void updateProgress(string label, double progress);
        void Play(PlaylistInfo fileInfo, bool from_start = false);
        void Stop();
        void Next();
        void Shuffle();
    }
}
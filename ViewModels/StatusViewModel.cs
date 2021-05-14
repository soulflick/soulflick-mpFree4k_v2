using MpFree4k.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MpFree4k.ViewModels
{
    public class StatusViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public void Update(double progress = -999)
        {
            if (pvm.Tracks.Count <= 0)
            {
                NumberOfTracks = 0;
                TotalDuration = "00:00";
                return;
            }

            NumberOfTracks = (ulong)pvm.Tracks.Count;

            TimeSpan span = new TimeSpan();
            foreach (PlaylistItem i in pvm.Tracks)
            {
                TimeSpan p = Utilities.LibraryUtils.GetDuration(i.Duration);
                span = span.Add(p);
            }

            TotalDuration = Utilities.LibraryUtils.GetDurationString((int)span.TotalSeconds);

            span = new TimeSpan();
            for (int i = pvm.CurrentPlayPosition; i < pvm.Tracks.Count; i++)
            {
                TimeSpan p = Utilities.LibraryUtils.GetDuration(pvm.Tracks[i].Duration);
                span = span.Add(p);
            }

            double secs = span.TotalSeconds;
            if (progress >= 0)
                secs -= progress;

            if (secs < 0)
                secs = 0;

            Remaining = Utilities.LibraryUtils.GetDurationString((int)secs);
        }

        private ulong _numberOfTracks = 0;
        public ulong NumberOfTracks
        {
            get => _numberOfTracks;
            set
            {
                _numberOfTracks = value;
                OnPropertyChanged("NumberOfTracks");
            }
        }

        private string _totalDuration = "00:00";
        public string TotalDuration
        {
            get => _totalDuration;
            set
            {
                _totalDuration = value;
                OnPropertyChanged("TotalDuration");
            }
        }

        private string _remaining = "00:00";
        public string Remaining
        {
            get => _remaining;
            set
            {
                _remaining = value;
                OnPropertyChanged("Remaining");
            }
        }

        public void Set(PlaylistViewModel PVM)
        {
            if (pvm != null)
                pvm.PropertyChanged -= PVM_PropertyChanged;

            PVM.PropertyChanged += PVM_PropertyChanged;

            pvm = PVM;
        }

        private PlaylistViewModel pvm = null;
        private void PVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Tracks")
            {
                Update();
            }
        }

    }
}

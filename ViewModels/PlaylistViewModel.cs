using MpFree4k.Classes;
using MpFree4k.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MpFree4k.ViewModels
{
    public enum PlayState
    {
        Play,
        PlayFromStart
    }

    public class PlaylistViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public StatusViewModel StatusVM { get; set; }
        public PlaylistViewModel()
        {
            StatusVM = new StatusViewModel();
            StatusVM.Set(this);
        }

        private List<PlaylistItem> _tracks = new List<PlaylistItem>();
        public List<PlaylistItem> Tracks
        {
            get { return _tracks; }
            set
            {
                _tracks = value;
                OnPropertyChanged("Tracks");
                StatusVM.Update();
            }
        }

        public void UnDrag()
        {
            foreach (var item in Tracks)
                item.DragOver = false;

            OnPropertyChanged("Tracks");
        }

        public static void Add(PlaylistItem[] items)
        {
            PlaylistViewModel VM = (MainWindow._singleton).Playlist.DataContext as PlaylistViewModel;
            VM.Add(items.ToList());
        }

        public static void Insert(PlaylistItem[] items)
        {
            PlaylistViewModel VM = (MainWindow._singleton).Playlist.DataContext as PlaylistViewModel;
            int idx = VM.CurrentPlayPosition;

            foreach (var p_i in items)
            {
                idx++;
                VM.Add(new List<PlaylistItem>() { p_i }, idx);
            }
        }

        public static void Play(PlaylistItem[] items)
        {
            PlaylistViewModel VM = (MainWindow._singleton).Playlist.DataContext as PlaylistViewModel;
            int playpos = VM.CurrentPlayPosition;
            int currentplaypos = -1;
            bool first = true;

            foreach (var p_i in items)
            {
                if (VM.Tracks.Count > 0)
                {
                    if (VM.Tracks.Count <= playpos)
                        playpos = VM.Tracks.Count;
                    else
                        playpos++;
                }
                else
                    playpos = 0;

                VM.Add(new List<PlaylistItem>() { p_i }, playpos);

                if (first)
                {
                    currentplaypos = playpos;
                    VM.CurrentPlayPosition = currentplaypos;
                    VM.Invoke(PlayState.Play);
                }

                first = false;
            }

            if (currentplaypos >= 0)
                VM.enumerate(currentplaypos);
        }

        public void Move(List<PlaylistItem> itms, int pos)
        {
            int minpos = Math.Min(itms.Min(i => i.Position) - 1, pos - 1);

            pos = Math.Min(pos, Tracks.Count);

            PlaylistItem referenceItem = pos < Tracks.Count && pos >= 0 ? Tracks[pos] : null;

            foreach (PlaylistItem pi in itms)
                Tracks.Remove(pi);

            enumerate(minpos);

            pos = referenceItem != null ? referenceItem.Position - 1 : Tracks.Count;

            Add(itms, pos);
            enumerate(minpos);
        }

        public void Add(PlaylistItem item, int position = int.MaxValue)
        {
            MainWindow.mainDispatcher.Invoke(() =>
            {

                if (position == int.MaxValue || position > Tracks.Count)
                    position = (int)Tracks.Count;

                Tracks.Insert(position, item);
            });
        }

        public void FinalizeAdd(int startpos = 0)
        {
            enumerate(startpos);
            foreach (var item in Tracks)
                item.DragOver = false;
        }

        public void Add(List<PlaylistItem> items, int position = int.MaxValue)
        {
            if (position == int.MaxValue || position > Tracks.Count)
                position = (int)Tracks.Count;

            int startpos = position;

            foreach (PlaylistItem item in items)
            {
                PlaylistItem new_item = new PlaylistItem()
                {
                    Album = item.Album,
                    Title = item.Title,
                    Duration = item.Duration,
                    Artists = item.Artists,
                    Path = item.Path,
                    Year = item.Year,
                    Track = item.Track,
                    uniqueID = item.uniqueID,
                    IsPlaying = item.IsPlaying
                };
                Tracks.Insert((int)position, new_item);
                position++;
            }

            UnDrag();
            enumerate(startpos);
        }

        public void Remove(int[] idxs)
        {
            if (idxs.Length == 0)
                return;

            int start = idxs[0];

            for (int i = idxs.Length - 1; i >= 0; i--)
            {
                int idx_p = idxs[i];
                if (Tracks.Count <= idx_p)
                    continue;

                Tracks.RemoveAt((int)idx_p);
            }

            enumerate(start);

            OnPropertyChanged("Tracks");
        }

        public void enumerate(int start)
        {
            start = Math.Max(start, 0);

            for (int i = start; i < Tracks.Count; i++)
            {
                Tracks[(int)i]._position = i + 1;
                Tracks[(int)i].TrackNumber = (i + 1).ToString();
            }

            if (CurrentSong != null)
            {
                PlaylistItem current = Tracks.FirstOrDefault(x => x.uniqueID == CurrentSong.uniqueID);
                if (current != null)
                    CurrentPlayPosition = Math.Max(0, current._position - 1);
            }

        }

        public RepeatMode RepeatMode = RepeatMode.GoThrough;

        private int _currentPlayPosition = 0;
        public int CurrentPlayPosition
        {
            get { return _currentPlayPosition; }
            set
            {
                _currentPlayPosition = value;

                for (int i = 0; i < Tracks.Count; i++)
                    Tracks[i].IsPlaying = false;

                if (value < 0 || value >= Tracks.Count)
                    return;

                Tracks[value].IsPlaying = true;
            }
        }

        public PlaylistItem CurrentSong = null;

        public PlaylistItem GetCurrent()
        {
            if (CurrentPlayPosition >= Tracks.Count)
                CurrentPlayPosition = Tracks.Count - 1;

            if (CurrentPlayPosition < 0)
                CurrentPlayPosition = 0;

            if (Tracks.Count == 0)
                return null;

            CurrentSong = Tracks[CurrentPlayPosition];
            return CurrentSong;
        }

        public void UpdatePlayPosition()
        {

            return;
            if (CurrentSong != null)
                CurrentPlayPosition = CurrentSong._position - 1;

        }

        public PlaylistItem GetShuffle()
        {
            Random rnd = new Random();
            int idx = rnd.Next(0, Tracks.Count);
            CurrentPlayPosition = idx;
            return GetCurrent();
        }

        public PlaylistItem GetNext()
        {
            if (RepeatMode == RepeatMode.GoThrough)
            {
                if (CurrentPlayPosition >= Tracks.Count - 1)
                {
                    CurrentPlayPosition = Tracks.Count - 1;
                    return null;
                }
                else
                {
                    CurrentPlayPosition++;
                }
            }
            else if (RepeatMode == RepeatMode.Loop)
            {
                if (CurrentPlayPosition >= Tracks.Count - 1)
                {
                    CurrentPlayPosition = 0;
                }
                else
                {
                    CurrentPlayPosition++;
                }
            }
            else if (RepeatMode == RepeatMode.Once)
            {
                return null;
                if (CurrentPlayPosition >= Tracks.Count - 1)
                {
                    CurrentPlayPosition = Tracks.Count - 1;
                    //return null;
                }
                else
                {
                    CurrentPlayPosition++;
                }
            }
            else if (RepeatMode == RepeatMode.RepeatOne)
            {
                if (CurrentPlayPosition >= Tracks.Count - 1)
                {
                    CurrentPlayPosition = Tracks.Count - 1;
                    //return null;
                }
                else
                {
                    CurrentPlayPosition++;
                }
            }
            else if (RepeatMode == RepeatMode.Shuffle)
            {
                return GetShuffle();
            }

            return GetCurrent();
        }

        public PlaylistItem GetPrevious()
        {
            if (RepeatMode == RepeatMode.GoThrough)
            {
                if (CurrentPlayPosition <= 0)
                {
                    CurrentPlayPosition = 0;
                }
                else
                {
                    CurrentPlayPosition--;
                }
            }
            else if (RepeatMode == RepeatMode.Loop)
            {
                if (CurrentPlayPosition <= 0)
                {
                    CurrentPlayPosition = 0;
                }
                else
                {
                    CurrentPlayPosition--;
                }
            }
            else if (RepeatMode == RepeatMode.Once)
            {
                if (CurrentPlayPosition <= 0)
                {
                    CurrentPlayPosition = 0;
                }
                else
                {
                    CurrentPlayPosition--;
                }
            }
            else if (RepeatMode == RepeatMode.RepeatOne)
            {
                if (CurrentPlayPosition <= 0)
                {
                    CurrentPlayPosition = 0;
                }
                else
                {
                    CurrentPlayPosition--;
                }
            }
            else if (RepeatMode == RepeatMode.Shuffle)
            {
                return GetShuffle();
            }

            return GetCurrent();
        }

        public void Invoke(PlayState state)
        {
            if (state == PlayState.Play)
                OnPropertyChanged("Play");
            else if (state == PlayState.PlayFromStart)
                OnPropertyChanged("PlayFromStart");
        }
    }
}

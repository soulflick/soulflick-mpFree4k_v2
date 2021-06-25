using Mpfree4k.Enums;
using Models;
using MpFree4k;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ViewModels
{
    public enum PlayState
    {
        Play,
        PlayFromStart
    }

    public class PlaylistViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public static PlaylistViewModel Instance = null;

        public StatusViewModel StatusVM { get; set; }
        public PlaylistViewModel()
        {
            StatusVM = new StatusViewModel();
            StatusVM.SetViewmodel(this);
            Instance = this;
        }

        public RepeatMode RepeatMode = RepeatMode.GoThrough;
        public PlaylistInfo CurrentSong = null;

        private int _currentPlayPosition = 0;
        public int CurrentPlayPosition
        {
            get => _currentPlayPosition;
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

        private List<PlaylistInfo> _tracks = new List<PlaylistInfo>();
        public List<PlaylistInfo> Tracks
        {
            get => _tracks;
            set
            {
                _tracks = value;
                Raise(nameof(Tracks));
                StatusVM.Update();
            }
        }

        public void UnDrag()
        {
            foreach (var item in Tracks)
                item.DragOver = false;

            Raise(nameof(Tracks));
        }

        public static void AddTracks(FileViewInfo[] infos) => Instance.Add(infos);

        public void Add(FileViewInfo[] infos)
        {
            if (infos == null || infos.Length == 0) return;
            Add(Utilities.LibraryUtils.GetItems(infos.Select(i => i.Path).ToArray()).ToList());
        }

        public static void Add(PlaylistInfo[] items)
        {
            PlaylistViewModel VM = (MainWindow.Instance).Playlist.DataContext as PlaylistViewModel;
            VM.Add(items.ToList());
        }

        public static void Insert(FileViewInfo[] infos)
        {
            var pItems = Utilities.LibraryUtils.GetItems(infos.Select(i => i.Path).ToArray()).ToArray();
            Insert(pItems);
        }

        public static void Insert(PlaylistInfo[] items)
        {
            PlaylistViewModel VM = (MainWindow.Instance).Playlist.DataContext as PlaylistViewModel;
            int idx = VM.CurrentPlayPosition;

            foreach (var p_i in items)
            {
                idx++;
                VM.Add(new List<PlaylistInfo>() { p_i }, idx);
            }
        }

        public static void Play(FileViewInfo[] items)
        {
            PlaylistViewModel VM = (MainWindow.Instance).Playlist.DataContext as PlaylistViewModel;
            var pItems = Utilities.LibraryUtils.GetItems(items.Select(s => s.Path).ToArray()).ToArray();
            Play(pItems);
        }

        public static void Play(PlaylistInfo[] items)
        {
            PlaylistViewModel VM = (MainWindow.Instance).Playlist.DataContext as PlaylistViewModel;
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

                VM.Add(new List<PlaylistInfo>() { p_i }, playpos);

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

        public void Move(List<PlaylistInfo> itms, int pos)
        {
            int minpos = Math.Min(itms.Min(i => i.Position) - 1, pos - 1);

            pos = Math.Min(pos, Tracks.Count);

            PlaylistInfo referenceItem = pos < Tracks.Count && pos >= 0 ? Tracks[pos] : null;

            foreach (PlaylistInfo pi in itms)
                Tracks.Remove(pi);

            enumerate(minpos);

            pos = referenceItem != null ? referenceItem.Position - 1 : Tracks.Count;

            Add(itms, pos);
            enumerate(minpos);
        }

        public void Add(PlaylistInfo item, int position = int.MaxValue)
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

        public void Add(List<PlaylistInfo> items, int position = int.MaxValue)
        {
            if (position == int.MaxValue || position > Tracks.Count)
                position = (int)Tracks.Count;

            int startpos = position;

            foreach (PlaylistInfo item in items)
            {
                PlaylistInfo new_item = new PlaylistInfo()
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

            Raise(nameof(Tracks));
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
                PlaylistInfo current = Tracks.FirstOrDefault(x => x.uniqueID == CurrentSong.uniqueID);
                if (current != null)
                    CurrentPlayPosition = Math.Max(0, current._position - 1);
            }

        }

        public PlaylistInfo GetCurrent()
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

        public PlaylistInfo GetShuffle()
        {
            Random rnd = new Random();
            int idx = rnd.Next(0, Tracks.Count);
            CurrentPlayPosition = idx;
            return GetCurrent();
        }

        public PlaylistInfo GetNext()
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

        public PlaylistInfo GetPrevious()
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
            if (state == PlayState.Play) Raise("Play");
            else if (state == PlayState.PlayFromStart) Raise("PlayFromStart");
        }
    }
}

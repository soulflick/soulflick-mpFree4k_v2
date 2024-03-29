﻿using Classes;
using Configuration;
using Layers;
using Models;
using MpFree4k;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;

namespace ViewModels
{
    public class FavouritesViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string info) => PropertyChanged(this, new PropertyChangedEventArgs(info));

        private List<SimpleAlbumItem> _favouriteAlbums = null;
        public List<SimpleAlbumItem> FavouriteAlbums
        {
            get => _favouriteAlbums;
            set
            {
                _favouriteAlbums = value;
                Raise(nameof(FavouriteAlbums));
            }
        }

        public List<FileViewInfo> LoadedTracks = new List<FileViewInfo>();

        private ObservableCollection<FileViewInfo> _favouriteTracks = null;
        public ObservableCollection<FileViewInfo> FavouriteTracks
        {
            get => _favouriteTracks;
            set
            {
                _favouriteTracks = value;
                Raise(nameof(FavouriteTracks));
            }
        }

        public void Load()
        {
            LoadAlbums();
            LoadTracks();
        }

        private void LoadTracks()
        {
            Thread load_thread = new Thread(() =>
            {
                List<FileViewInfo> _tsx = new List<FileViewInfo>();
                List<Tuple<string, int, DateTime>> _tx = Library.Instance.connector.GetRecentTracksDetails(UserConfig.NumberRecentTracks);

                MainWindow.SetProgress(0);
                double step = 100 / (_tx.Count + 1);
                int _count = 0;

                foreach (Tuple<string, int, DateTime> p in _tx)
                {
                    _count++;
                    MainWindow.SetProgress(step * _count);
                    FileViewInfo fin = new FileViewInfo(p.Item1);
                    fin.PlayCount = p.Item2;
                    fin.LastPlayed = p.Item3;

                    fin.CreateFileHandle();

                    if (fin._Handle == null)
                        continue;

                    BitmapImage img = MediaLibrary.GetImageFromTag(fin._Handle.Tag.Pictures);
                    if (img != null && fin.Image == null)
                        fin.Image = img;

                    _tsx.Add(fin);
                }

                LoadedTracks.Clear();
                LoadedTracks.AddRange(_tsx);

                FavouriteTracks = new ObservableCollection<FileViewInfo>(_tsx);
                
                MainWindow.SetProgress(0);

                TimeSpan span = TimeSpan.FromSeconds(_tsx.Sum(t => t.Mp3Fields.DurationValue));
                MainWindow.Instance.SetAmounts(_tsx.Count, span);

            });

            load_thread.Start();
        }

        public void LoadAlbums()
        {
            Thread load_thread = new Thread(() =>
            {
                List<Tuple<string, string, int>> _tx = Library.Instance.connector.GetRecentAlbums(UserConfig.NumberRecentAlbums);
                List<SimpleAlbumItem> _ta = new List<SimpleAlbumItem>();
                uint y = 0;
                int id = 0;

                double max = UserConfig.NumberRecentAlbums;
                double percent = 0;
                int num = -1;

                foreach (Tuple<string, string, int> i in _tx)
                {
                    num++;
                    percent = (100 / max) * num;

                    MainWindow.SetProgress(percent);

                    uint.TryParse(i.Item2, out y);
                    SimpleAlbumItem aitm = new SimpleAlbumItem() { AlbumLabel = i.Item1, Year = y };

                    id = i.Item3;
                    aitm.id = id;

                    string[] tracks = Library.Instance.connector.GetAlbumTracks(id);
                    aitm.Tracks = tracks;

                    int workingtrackscount = tracks.Count(t => File.Exists(t));
                    aitm.TrackCount = workingtrackscount;

                    if (workingtrackscount == 0)
                        continue;

                    foreach (string track in tracks)
                    {
                        if (aitm.HasImage)
                            break;

                        FileViewInfo fin = new FileViewInfo(track);
                        fin.CreateFileHandle();

                        if (fin._Handle == null)
                            continue;

                        if (string.IsNullOrEmpty(aitm.Artist))
                            aitm.Artist = fin.Mp3Fields.AlbumArtists;

                        BitmapImage img = MediaLibrary.GetImageFromTag(fin._Handle.Tag.Pictures);
                        if (img != null)
                        {
                            aitm.AlbumImage = img;
                            aitm.HasImage = true;
                            break;
                        }
                    }

                    if (!aitm.HasImage)
                        aitm.AlbumImage = MediaLibrary.DefaultAlbumImage;

                    if (string.IsNullOrEmpty(aitm.Artist))
                        aitm.Artist = "Unknown Artist";

                    _ta.Add(aitm);

                    MainWindow.SetProgress(0);

                }

                this.FavouriteAlbums = _ta;
            });

            load_thread.Start();
            load_thread.Join();


            MainWindow.SetProgress(0);
        }

        public void Remove(List<SimpleAlbumItem> ai)
        {
            foreach (SimpleAlbumItem sai in ai)
                Library.Instance.connector.RemoveAlbum(sai.id);
        }
    }
}

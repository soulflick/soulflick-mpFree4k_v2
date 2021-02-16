using Classes;
using MpFree4k.Classes;
using MpFree4k.Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media.Imaging;

namespace MpFree4k.ViewModels
{
    public class FavouritesViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private List<SimpleAlbumItem> _favouriteAlbums = null;
        public List<SimpleAlbumItem> FavouriteAlbums
        {
            get
            {
                return _favouriteAlbums;
            }
            set
            {
                _favouriteAlbums = value;
                OnPropertyChanged("FavouriteAlbums");
            }
        }

        private List<FileViewInfo> _favouriteTracks = null;
        public List<FileViewInfo> FavouriteTracks
        {
            get
            {
                return _favouriteTracks;
            }
            set
            {
                _favouriteTracks = value;
                OnPropertyChanged("FavouriteTracks");
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
                List<Tuple<string, int, DateTime>> _tx = Library._singleton.connector.GetRecentTracksDetails(UserConfig.NumberRecentTracks);

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
                FavouriteTracks = _tsx;
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
                List<Tuple<string, string, int>> _tx = Library._singleton.connector.GetRecentAlbums(UserConfig.NumberRecentAlbums);
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
                    //MainWindow.mainDispatcher.BeginInvoke(MainWindow.Instance.delegateUpdateProgress, new object[] { percent });

                    uint.TryParse(i.Item2, out y);
                    SimpleAlbumItem aitm = new SimpleAlbumItem() { AlbumLabel = i.Item1, Year = y };

                    id = i.Item3;
                    aitm.id = id;

                    //int count = Library._singleton.connector.GetAlbumTrackCount(id);
                    //aitm.TrackCount = count;

                    string[] tracks = Library._singleton.connector.GetAlbumTracks(id);
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
                Library._singleton.connector.RemoveAlbum(sai.id);
        }
    }
}

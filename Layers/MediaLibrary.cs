using Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Imaging;
using Configuration;
using Models;
using Mpfree4k.Enums;
using MpFree4k;

namespace Layers
{
    public class MediaLibrary : INotifyPropertyChanged
    {
        public static BitmapImage DefaultAlbumImage = null;
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };
        public string Name = "[not set]";
        public string LibPath = "[not set]";

        private string[] selected_artists = null;
        private List<AlbumItem> selected_albums = null;
        private Dispatcher current_dispatcher = null;
        private Task t_load;
        private int _filesCount = 0;

        public MediaLibrary()
        {
            current_dispatcher = Dispatcher.CurrentDispatcher;

            DefaultAlbumImage = new BitmapImage(
                new System.Uri(@"pack://application:,,,/" +
                System.Reflection.Assembly.GetCallingAssembly().GetName().Name +
                ";component/" + "Images/no_album_cover.jpg", System.UriKind.Absolute));
        }


        private string _statusText = "";
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                Raise(nameof(StatusText));
            }
        }

        public string _query = "";
        public string Query
        {
            get => _query;
            set
            {
                _query = value.ToLower();
                QueryMe(ViewMode.Details);
                Raise(nameof(Query));
            }
        }

        private List<ArtistItem> _artists = new List<ArtistItem>();
        public List<ArtistItem> Artists
        {
            get => _artists;
            set
            {
                _artists = value;
                Raise(nameof(Artists));
            }
        }

        private List<AlbumItem> _albums = new List<AlbumItem>();
        public List<AlbumItem> Albums
        {
            get => _albums;
            set
            {
                _albums = value;
                Raise(nameof(Albums));
            }
        }

        private List<FileViewInfo> _files = new List<FileViewInfo>();
        public List<FileViewInfo> Files
        {
            get => _files;
            set
            {
                _files = value;
                Raise(nameof(Files));
            }
        }


        private int _artistCount = 0;
        public int ArtistCount
        {
            get => _artistCount;
            set
            {
                _artistCount = value;
                Raise(nameof(ArtistCount));

            }
        }

        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public void Reset()
        {
            Artists.Clear();
            Albums.Clear();
            Files.Clear();
            Refresh(MediaLevel.All);
        }

        public void Refresh(MediaLevel mediaLevel)
        {
            switch (mediaLevel)
            {
                case MediaLevel.Artists:
                    Raise("Artists");
                    break;
                case MediaLevel.Albums:
                    Raise("Albums");
                    break;
                case MediaLevel.Tracks:
                    Raise("Tracks");
                    break;
                case MediaLevel.All:
                    Raise("Artists");
                    Raise("Albums");
                    Raise("Files");
                    Raise("Tracks");
                    break;
            }
        }

        private bool hasKey(List<Tuple<string, string>> tokens, string key) => tokens.Any(t => t.Item1.ToLower() == key.ToLower());

        private string getValue(List<Tuple<string, string>> tokens, string key)
        {
            if (!hasKey(tokens, key)) return null;
            return tokens.FirstOrDefault(t => t.Item1 == key).Item2.Trim();
        }


        public void QueryExplicit(ViewMode viewmode)
        {
            List<Tuple<string, string>> tokens = new List<Tuple<string, string>>();

            string _thisQuery = _query.Trim().ToLower();
            string _q = _thisQuery;
            char delim = ':';
            int idx = _q.IndexOf(delim);
            string key = "", value = "";

            while (idx >= 0)
            {
                string before = _q.Substring(0, idx);
                if (string.IsNullOrWhiteSpace(before))
                {
                    _q = _q.Substring(idx + 1);
                    idx = _q.IndexOf(delim);
                    continue;
                }

                int idx2 = before.LastIndexOf(' ');

                if (!string.IsNullOrWhiteSpace(key))
                {
                    if (idx2 >= 0)
                        value = before.Substring(0, idx2);
                    else
                        value = before;

                    tokens.Add(new Tuple<string, string>(key, value));
                    key = "";
                    value = "";

                    if (idx2 >= 0)
                        key = before.Substring(idx2 + 1);
                }
                else
                {
                    if (idx2 >= 0)
                        key = before.Substring(idx2 + 1);
                    else
                        key = before;
                }

                _q = _q.Substring(idx + 1);
                idx = _q.IndexOf(delim);
            }

            _q = _q.Trim();
            key = key.Trim();

            if (!string.IsNullOrEmpty(_q))
            {
                if (!string.IsNullOrEmpty(key))
                {
                    value = _q;
                    tokens.Add(new Tuple<string, string>(key, value));
                }
            }

            string title = getValue(tokens, "title");
            string album = getValue(tokens, "album");
            string artist = getValue(tokens, "artist");
            string flag = getValue(tokens, "flag");

            if (string.IsNullOrEmpty(title))
                title = getValue(tokens, "t");
            if (string.IsNullOrEmpty(title))
                title = getValue(tokens, "ti");

            if (string.IsNullOrEmpty(album))
                album = getValue(tokens, "al");
            if (string.IsNullOrEmpty(album))
                album = getValue(tokens, "alb");

            if (string.IsNullOrEmpty(artist))
                artist = getValue(tokens, "ar");
            if (string.IsNullOrEmpty(artist))
                artist = getValue(tokens, "art");

            long year = -999;
            long track = -999;
            FlagType? flagType = null;
            string flagString = string.Empty;

            if (_thisQuery.StartsWith("flag:") || _thisQuery.Contains(" flag:")) flag = "flag:";

            if (hasKey(tokens, "flag"))
            {
                flagString = getValue(tokens, "flag");
                if (FlagType.TryParse(flagString, out FlagType _flagType)) { flagType = _flagType; }
            }

            if (hasKey(tokens, "track"))
                track = Convert.ToInt64(getValue(tokens, "track"));

            if (hasKey(tokens, "year"))
                year = Convert.ToInt64(getValue(tokens, "year"));

            bool anyset = !string.IsNullOrEmpty(artist) ||
                !string.IsNullOrEmpty(album) ||
                !string.IsNullOrEmpty(title) ||
                track != -999;

            if (viewmode == ViewMode.Table)
            {
                Files.ForEach(f => f.IsVisible = true);

                if (!string.IsNullOrEmpty(flag))
                {
                    FlagType? fType = null;
                    if (!string.IsNullOrEmpty(flagString))
                    {
                        fType = Utilities.EnumUtils.GetFlagType(flagString);
                        if (fType != null) flagType = (FlagType)fType;
                    }
                    if (flagType == null)
                        Files.Where(f => f.IsVisible).ToList().ForEach(f => f.IsVisible = f.Flag > 0);
                    else
                        Files.Where(f => f.IsVisible).ToList().ForEach(f => f.IsVisible = f.Flag == flagType);

                    Refresh(MediaLevel.Tracks);
                    Raise("Filter");
                    return;

                }
                if (!string.IsNullOrEmpty(title))
                {
                    Files.Where(f => f.IsVisible).ToList().ForEach(f => f.IsVisible = f.Title.ToLower().Contains(title));
                }
                if (!string.IsNullOrEmpty(album))
                {
                    Files.Where(f => f.IsVisible).ToList().ForEach(f => f.IsVisible = f.Mp3Fields.Album.ToLower().Contains(album));
                }
                if (!string.IsNullOrEmpty(artist))
                {
                    Files.Where(f => f.IsVisible).ToList().ForEach(f => f.IsVisible = f.Mp3Fields.Artists.ToLower().Contains(artist));
                }
                if (track != -999)
                {
                    Files.Where(f => f.IsVisible).ToList().ForEach(f => f.IsVisible = f.Mp3Fields.Track == track);
                }
                if (year != -999)
                {
                    Files.Where(f => f.IsVisible).ToList().ForEach(f => f.IsVisible = f.Mp3Fields.Year == year);
                }

                Refresh(MediaLevel.Tracks);
            }
            else if (viewmode == ViewMode.Details)
            {
                if (!string.IsNullOrEmpty(artist))
                    Artists.ForEach(x => x.IsVisible = x.Artists.ToLower().Contains(artist));
                if (!string.IsNullOrEmpty(album))
                    Albums.ForEach(x => x.IsVisible = x.Album.ToLower().Contains(album));
                if (!string.IsNullOrEmpty(title))
                    Files.ForEach(x => x.IsVisible = x.Title.ToLower().Contains(title));

                Refresh(MediaLevel.Artists);
                Refresh(MediaLevel.Albums);
                Refresh(MediaLevel.Tracks);
            }
            else
            {
                Albums.ForEach(x => x.IsVisible = (
                (string.IsNullOrEmpty(artist) || !string.IsNullOrEmpty(artist) && x.AllArtist.Any(y => y.ToLower().Contains(artist)))) &&
                (string.IsNullOrEmpty(album) || !string.IsNullOrEmpty(album) && x.Album.ToLower().Contains(album)) &&
                (year == -999 || x.Year == year));


                Refresh(MediaLevel.Albums);
            }

            Raise("Filter");
        }
        public void QueryMe(ViewMode viewmode)
        {
            var _thisQuery = _query.Trim().ToLower();
            if (_thisQuery.Contains(':'))
            {
                QueryExplicit(viewmode);
                return;
            }

            List<string> query_strings = new List<string>();
            _thisQuery.Split(" ".ToCharArray()).ToList().
                Where(x => !string.IsNullOrEmpty(x)).ToList()
                .ForEach(f => query_strings.Add(f.ToLower()));

            if (viewmode == ViewMode.Table)
            {
                Files.ForEach(x => x.IsVisible =

                (string.IsNullOrEmpty(_thisQuery)) ||
                (!string.IsNullOrEmpty(x.Title) &&
                query_strings.Any(q => x.Title.ToLower().Contains(q)) ||
                query_strings.Any(q => x.Mp3Fields.Artists.ToLower().Contains(q)) ||
                query_strings.Any(q => x.Mp3Fields.Album.ToLower().Contains(q))));

                Refresh(MediaLevel.Tracks);
            }
            else if (viewmode == ViewMode.Details)
            {
                Artists.ForEach(x => x.IsVisible = string.IsNullOrEmpty(_thisQuery) || query_strings.Any(q => x.Artists.ToLower().Contains(q)));
                Albums.ForEach(x => x.IsVisible = string.IsNullOrEmpty(_thisQuery) || query_strings.Any(q => x.Album.ToLower().Contains(q)));
                Files.ForEach(x => x.IsVisible = string.IsNullOrEmpty(_thisQuery) || (string.IsNullOrEmpty(x.Title) && string.IsNullOrEmpty(_thisQuery)) ||
                (!string.IsNullOrEmpty(x.Title) && query_strings.Any(q => x.Title.ToLower().Contains(q))));

                Refresh(MediaLevel.Artists);
                Refresh(MediaLevel.Albums);
                Refresh(MediaLevel.Tracks);
            }
            else
            {
                Albums.ForEach(x => x.IsVisible = string.IsNullOrEmpty(_thisQuery) || (
                x.AllArtist.Any(y => query_strings.Any(q => y.ToLower().Contains(q))) ||
                query_strings.Any(q => x.Artist.ToLower().Contains(q)) ||
                query_strings.Any(q => x.Album.ToLower().Contains(q))));

                Refresh(MediaLevel.Albums);
            }

            Raise("Filter");
        }

        public TimeSpan GetVisibleLength()
        {
            if (Files == null)
                return new TimeSpan(0);

            double len = 0;

            ViewMode viewmode = MainWindow.Instance.ViewMode;
            if (viewmode == ViewMode.Albums)
                len = Albums.Where(a => a.IsVisible).Sum(a => a.Tracks.Sum(t => t.Length));
            else if (viewmode == ViewMode.Favourites)
                len = (MainWindow.Instance.Favourites.DataContext as ViewModels.FavouritesViewModel).FavouriteTracks.Where(f => f.IsVisible).Sum(t => t.Mp3Fields.DurationValue);
            else
                len = Files.Where(f => f.IsVisible).Sum(f => f.Mp3Fields.DurationValue);
            TimeSpan span = new TimeSpan(0);
            span += TimeSpan.FromSeconds(len);
            return span;

        }

        public int GetVisibleTracks()
        {
            if (Files == null)
                return 0;

            ViewMode viewmode = MainWindow.Instance.ViewMode;
            if (viewmode == ViewMode.Table)
                return Files.Count(f => f.IsVisible);
            else if (viewmode == ViewMode.Albums)
                return Albums.Count(f => f.IsVisible);
            else if (viewmode == ViewMode.Details)
                return Files.Count(f => f.IsVisible);
            else if (viewmode == ViewMode.Favourites)
                return (MainWindow.Instance.Favourites.DataContext as ViewModels.FavouritesViewModel).FavouriteTracks.Count(t => t.IsVisible);
            else
                return Files.Count(f => f.IsVisible);
        }

        public void ClearSelection()
        {
            selected_artists = null;
            if (selected_albums != null && selected_albums.Any())
                selected_albums.Clear();
        }

        public void Set(List<SimpleAlbumItem> albums)
        {
            Albums.ForEach(x => x.IsVisible =
            (albums.Any(a => x.Album == a.AlbumLabel && x.Year == a.Year)));

            Refresh(MediaLevel.Albums);

            Files.ForEach(x => x.IsVisible =
            (albums.Any(a => a.AlbumLabel == x.Mp3Fields.Album &&
            a.Year == x.Mp3Fields.Year &&
            (string.IsNullOrEmpty(a.Artist) || a.Artist == "Various Artists" || a.Artist == x.Mp3Fields.Artists))));

            Refresh(MediaLevel.All);
        }

        public void Set(SimpleAlbumItem album)
        {
            Albums.ForEach(x => x.IsVisible =
                (x.Album == album.AlbumLabel && x.Year == album.Year));

            Refresh(MediaLevel.Albums);

            Files.ForEach(x => x.IsVisible = (x.Mp3Fields.Album == album.AlbumLabel && x.Mp3Fields.Year == album.Year &&
                x.Mp3Fields.Artists == album.Artist));

            Refresh(MediaLevel.All);
        }

        public void Filter(MediaLevel level, List<AlbumItem> albumitems)
        {
            selected_albums = albumitems;
            if (albumitems != null && albumitems.Count > 0)
            {
                if (UserConfig.ShowFullAlbum)
                {
                    Files.ForEach(x => x.IsVisible = albumitems.Any(
                        album => album.Album == x.Mp3Fields.Album && album.Year == x.Mp3Fields.Year));
                }
                else
                {
                    Files.ForEach(x => x.IsVisible = albumitems.Any(
                        album => album.Album == x.Mp3Fields.Album && album.Year == x.Mp3Fields.Year &&
                        (
                        string.IsNullOrEmpty(album.Artist) ||
                        selected_artists == null ||
                        selected_artists.Length == 0 ||
                        selected_artists.Any(s => s == x.Mp3Fields.AlbumArtists))));
                }
            }
            else if (selected_artists == null || selected_artists.Length == 0)
            {
                for (int f = 0; f < Files.Count; f++)
                {
                    if (!Files[f].IsVisible)
                        Files[f].IsVisible = true;
                }
            }
            else Files.ForEach(x => x.IsVisible = selected_artists.Any(artist => artist == x.Mp3Fields.Artists));

            Refresh(MediaLevel.Tracks);


        }

        public void Filter(MediaLevel level, string[] selection = null)
        {
            if (level == MediaLevel.Artists)
            {
                if (selection != null)
                    selected_artists = selection;

                Albums.ForEach(x => x.IsVisible =
                (selected_artists == null || selected_artists.Length == 0 || (x.AllArtist.Any(y => selected_artists.Any(a => y == a)))));

                if (UserConfig.RememberSelectedAlbums)
                {
                    Albums.ForEach(x => x.IsVisible = x.IsSelected ? true : x.IsVisible);
                    Albums.ForEach(x => x.IsSpecialVisible = x.IsVisible);
                }

                Refresh(MediaLevel.Albums);

                Files.ForEach(x => x.IsVisible = selected_artists == null || selected_artists.Length == 0 || (selected_artists.Any(a => a == x.Mp3Fields.Artists)));
                Refresh(MediaLevel.Tracks);

                if (selected_albums != null && selected_albums.Count > 0)
                    Files.ForEach(x => x.IsVisible = selected_albums.Any(album => album.Album == x.Mp3Fields.Album &&
                    (selected_artists == null || selected_artists.Length == 0 || selected_artists.Any(a => a == x.Mp3Fields.Artists))));
                else if (selected_artists == null || selected_artists.Length == 0)
                    Files.ForEach(x => x.IsVisible = true);
                else
                    Files.ForEach(x => x.IsVisible = selected_artists.Any(artist => artist == x.Mp3Fields.Artists));

                Filter(MediaLevel.Albums, selected_albums);
                Raise("IndexAlbums");

            }
            else if (level == MediaLevel.Albums)
            {
                return; 

                if (selection.Length > 0)
                    Files.ForEach(x => x.IsVisible = selection.Any(album => album == x.Mp3Fields.Album &&
                    (selected_artists == null || selected_artists.Length == 0 || selected_artists.Any(a => a == x.Mp3Fields.Artists))));
                else if (selected_artists == null || selected_artists.Length == 0)
                    Files.ForEach(x => x.IsVisible = true);
                else
                    Files.ForEach(x => x.IsVisible = selected_artists.Any(artist => artist == x.Mp3Fields.Artists));

                Refresh(MediaLevel.Tracks);
            }
        }

        public void Load()
        {

            int numfiles = GetFilesCount(LibPath);
            if (numfiles <= 0)
                return;

            MainWindow.Instance.IsEnabled = false;

            _filesCount = 0;
            double percent = 0;

            t_load = new Task(() => Load(LibPath));
            Thread t = new Thread(() =>
            {
                while (!t_load.IsCompleted)
                {
                    Thread.Sleep(Config.update_timeout);

                    percent = (_filesCount * 100) / (numfiles * 2);

                    MainWindow.SetProgress(percent);
                }

                t_load = new Task(() => tagImages());
                t_load.Start();

                while (!t_load.IsCompleted)
                {
                    Thread.Sleep(Config.update_timeout);

                    percent = (_filesCount * 100) / (numfiles * 2);

                    MainWindow.SetProgress(percent);
                }

                StatusText = "ordering library...";

                MainWindow.mainDispatcher.BeginInvoke(
                    DispatcherPriority.Render, (Action)(() =>
                    {
                        lock (Artists)
                        {
                            Artists = Artists.OrderBy(a => a.Artists).ToList();
                            Raise("Artists");
                        }
                    }));

                MainWindow.mainDispatcher.BeginInvoke(
                   DispatcherPriority.Background, (Action)(() =>
                   {
                       lock (Albums)
                       {
                           Albums = Albums.OrderBy(a => a.Album).ThenBy(a => a.Album).ToList();
                           Raise("Albums");
                       }
                   }));

                MainWindow.mainDispatcher.BeginInvoke(
                   DispatcherPriority.Background, (Action)(() =>
                   {
                       lock (Files)
                       {
                           Files = Files.OrderBy(a => a.Mp3Fields.Album).ThenBy(b => b.Mp3Fields.Track).ThenBy(c => c.Mp3Fields.Artists).ToList();
                           Raise("Tracks");
                       }
                   }));

                MainWindow.mainDispatcher.Invoke(
                    DispatcherPriority.Background, (Action)(() =>
                    {
                        StatusText = "done (" + Files.Count.ToString() + ")";
                    }));

                MainWindow.SetProgress(0);
            });
            t_load.Start();
            t.Start();

        }

        public int GetFilesCount(string path, int numfiles = 0)
        {
            int count = numfiles;
            if (!Directory.Exists(path))
                return 0;

            count += Directory.GetFiles(path).Count(file => Config.media_extensions.Any(x => x == Path.GetExtension(file)));

            foreach (string dir in Directory.GetDirectories(path))
            {
                count += GetFilesCount(dir, 0);
            }
            return count;
        }

        public static BitmapImage GetImageFromTag(TagLib.IPicture[] Pictures)
        {
            if (Pictures.Length == 0 || Pictures[0] == null)
            {
                return null;
            }

            byte[] raw = Pictures[0].Data.ToArray();
            BitmapImage _img = new BitmapImage();

            try
            {
                _img.BeginInit();
                _img.UriSource = null;
                _img.BaseUri = null;
                _img.StreamSource = new MemoryStream(raw);
                _img.EndInit();
                _img.Freeze();
                return _img;
            }
            catch (NotSupportedException nsExc)
            {
                _img = null;
                return _img;
            }
            catch (Exception exc)
            {
                _img = null;
                return _img;
            }
        }

        private void tagImages()
        {
            string msg = "loading image {0} from {1}";
            int max = Files.Count;
            int index = 0;

            foreach (FileViewInfo i in Files)
            {
                index++;
                _filesCount++;

                if (i._Handle == null) continue;

                BitmapImage img = GetImageFromTag(i._Handle.Tag.Pictures);
                AlbumItem a = Albums.FirstOrDefault(x => (x.Artist == i.Mp3Fields.Artists || x.Artist == "Various Artists") && x.Album == i.Mp3Fields.Album &&
                x.Year == i.Mp3Fields.Year);

                if (a != null)
                {
                    if (img != null && !a.HasAlbumImage)
                    {
                        a.AlbumImage = img;
                        a.HasAlbumImage = true;
                    }

                }

                ArtistItem artist = Artists.FirstOrDefault(x => x.Artists == i.Mp3Fields.Artists && x.FirstAlbum == null);
                if (artist != null)
                {
                    artist.FirstAlbum = img;
                    if (a != null)
                    {
                        SimpleAlbumItem sai = artist.Albums.FirstOrDefault(x => (string.IsNullOrEmpty(x.AlbumLabel) || x.AlbumLabel == a.Album) && x.Year == a.Year);
                        if (sai != null) // must always happen
                        {
                            if (sai.AlbumImage == null)
                                sai.AlbumImage = img;
                        }
                    }

                }
                i.TrackImage = img;
            }

            foreach (AlbumItem a in Albums)
            {
                if (!a.HasAlbumImage)
                    a.AlbumImage = DefaultAlbumImage;
            }
        }

        public void Load(string path)
        {
            Parallel.ForEach(Directory.GetFiles(path), (file) =>
            {
                if (Config.media_extensions.Any(x => x == Path.GetExtension(file)))
                {
                    FileViewInfo fin = new FileViewInfo(file);
                    fin.CreateFileHandle();

                    if (string.IsNullOrEmpty(fin.Title))
                    {
                        string filename = Path.GetFileNameWithoutExtension(fin.Path);
                        fin.Title = filename;
                        fin.Mp3Fields.Title = filename;
                    }


                    addFile(fin);
                };
            });
            Parallel.ForEach(Directory.GetDirectories(path), (dir) =>
            {
                Load(dir);
            });
        }

        private void addFile(FileViewInfo fin)
        {
            lock (Files)
            {
                _filesCount++;
                Files.Add(fin);
            }
            lock (Artists)
            {
                ArtistItem ai = Artists.FirstOrDefault(a => a.Artists == fin.Mp3Fields.Artists);
                if (ai == null)
                {
                    Artists.Add(new ArtistItem()
                    {
                        Artists = fin.Mp3Fields.Artists,
                        TrackCount = 1,
                        AlbumCount = 1,
                        Albums = new List<SimpleAlbumItem>()
                        {
                            new SimpleAlbumItem() {
                                AlbumLabel = fin.Mp3Fields.Album,
                                Year = fin.Mp3Fields.Year,
                                Genre = fin.Mp3Fields.Genres,
                                TrackCount = 1
                            }
                        }
                    });
                    AlbumItem album_item = Albums.FirstOrDefault(x => x.Album == fin.Mp3Fields.Album && x.Year == fin.Mp3Fields.Year);
                    if (album_item != null)
                    {
                        lock (album_item)
                        {
                            album_item.TrackCount++;
                            if (!album_item.AllArtist.Any(x => x == fin.Mp3Fields.Artists))
                            {
                                album_item.Artist = "Various Artists";
                                album_item.AllArtist.Add(fin.Mp3Fields.Artists);
                                album_item.Tracks.Add(fin.Path);
                            }
                        }
                    }
                    else
                    {
                        lock (Albums)
                        {
                            AlbumItem albi = new AlbumItem()
                            {
                                TrackCount = 1,
                                Album = fin.Mp3Fields.Album,
                                Year = fin.Mp3Fields.Year,
                                Artist = fin.Mp3Fields.Artists,
                                Genre = fin.Mp3Fields.Genres,
                                AllArtist = new List<string>() { fin.Mp3Fields.Artists }
                            };
                            albi.Tracks.Add(fin.Path);

                            Albums.Add(albi);
                        }
                    }
                    ArtistCount++;
                }
                else
                {
                    ai.TrackCount++;
                    if (!ai.Albums.Any(album => album.AlbumLabel == fin.Mp3Fields.Album))
                    {
                        ai.Albums.Add(
                             new SimpleAlbumItem()
                             {
                                 AlbumLabel = fin.Mp3Fields.Album,
                                 Year = fin.Mp3Fields.Year,
                                 Genre = fin.Mp3Fields.Genres,
                                 TrackCount = 1
                             });
                        ai.AlbumCount++;

                        AlbumItem album_item = Albums.FirstOrDefault(x => x.Album == fin.Mp3Fields.Album && x.Year == fin.Mp3Fields.Year);
                        if (album_item != null)
                        {
                            album_item.TrackCount++;
                            if (!album_item.AllArtist.Any(x => x == fin.Mp3Fields.Artists))
                            {
                                album_item.Artist = "Various Artists";
                                album_item.AllArtist.Add(fin.Mp3Fields.Artists);
                            }
                            album_item.Tracks.Add(fin.Path);
                        }
                        else
                        {
                            lock (Albums)
                            {
                                MainWindow.mainDispatcher.Invoke(() =>
                                {
                                    AlbumItem album = new AlbumItem()
                                    {
                                        TrackCount = 1,
                                        Album = fin.Mp3Fields.Album,
                                        Year = fin.Mp3Fields.Year,
                                        Artist = fin.Mp3Fields.Artists,
                                        Genre = fin.Mp3Fields.Genres,
                                        AllArtist = new List<string>() { fin.Mp3Fields.Artists }
                                    };
                                    album.Tracks.Add(fin.Path);


                                    Albums.Add(album);
                                });
                            }
                        }
                    }
                    else
                    {
                        SimpleAlbumItem sab = ai.Albums.FirstOrDefault(album => album.AlbumLabel == fin.Mp3Fields.Album);
                        if (sab.Year <= 0)
                            sab.Year = fin.Mp3Fields.Year;
                        sab.TrackCount++;

                        AlbumItem album_item = Albums.FirstOrDefault(x => x.Album == fin.Mp3Fields.Album && x.Year == fin.Mp3Fields.Year);

                        if (album_item != null)
                        {
                            album_item.TrackCount++;
                            if (!album_item.AllArtist.Any(x => x == fin.Mp3Fields.Artists))
                            {
                                album_item.Artist = "Various Artists";
                                album_item.AllArtist.Add(fin.Mp3Fields.Artists);
                            }
                            album_item.Tracks.Add(fin.Path);
                        }
                    }
                }
            }
        }
    }
}

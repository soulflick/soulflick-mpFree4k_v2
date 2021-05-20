using Mpfree4k.Enums;
using Layers;
using Models;
using MpFree4k;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ViewModels
{

    public class AlbumDetailGroup : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public string JumpNode = "";

        private string _groupName = "";
        public string GroupName
        {
            get => _groupName;
            set
            {
                _groupName = value;
                Raise(nameof(GroupName));
            }
        }

        private AlbumItem[] _albums = null;
        public AlbumItem[] Albums
        {
            get => _albums;
            set
            {
                _albums = value;
                Raise(nameof(Albums));
            }
        }
    }
    public class AlbumDetailViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        //public bool loaded;
        private AlbumDetailsOrderType ViewType = AlbumDetailsOrderType.Year;

        private List<AlbumDetailGroup> _albumGroups = new List<AlbumDetailGroup>();
        public List<AlbumDetailGroup> AlbumGroups
        {
            get =>_albumGroups;
            set
            {
                _albumGroups = value;
                //if (_albumGroups.Count > 0 && _albumGroups[0].Albums.Length > 0)
                //    loaded = true;

                Raise(nameof(AlbumGroups));
            }
        }

        List<AlbumItem> _albums = new List<AlbumItem>();
        public List<AlbumItem> Albums
        {
            get => _albums;
            set
            {
                _albums = value;
                Raise(nameof(Albums));
            }
        }

        public AlbumDetailViewModel() => Library.Instance.Current.PropertyChanged += Current_PropertyChanged;

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Albums))
                OrderBy(ViewType);
        }

        public void Reload() => OrderBy(ViewType);

        void getGroups(AlbumDetailsOrderType type = AlbumDetailsOrderType.Album)
        {
            if (MainWindow.Instance.ViewMode != ViewMode.Albums) return;

            AlbumGroups.Clear();

            List<AlbumDetailGroup> _groups = new List<AlbumDetailGroup>();

            if (type == AlbumDetailsOrderType.Album)
            {
                var groups = Albums.Where(x => !string.IsNullOrEmpty(x.Album)).GroupBy(a => a.Album[0]);
                double step_width = 100 / Math.Max(1, groups.Count());
                double step = 0;
                
                foreach (var group in groups)
                {
                    step += step_width;
                    MainWindow.SetProgress(step);

                    string albumname = group.First().Album;
                    AlbumDetailGroup g = new AlbumDetailGroup()
                    {
                        GroupName = albumname,
                        JumpNode = string.IsNullOrEmpty(albumname) ? "" : albumname[0].ToString()
                    };
                    g.Albums = group.OrderBy(c => c.Album).ToArray();
                    _groups.Add(g);
                }
            }
            else if (type == AlbumDetailsOrderType.Artist)
            {
                var groups = Albums.Where(x => !string.IsNullOrEmpty(x.Artist)).GroupBy(a => a.Artist[0]);
                double step_width = 100 / Math.Max(1, groups.Count());
                double step = 0;

                foreach (var group in groups)
                {
                    step += step_width;
                    MainWindow.SetProgress(step);

                    string artist = group.First().Artist;
                    AlbumDetailGroup g = new AlbumDetailGroup()
                    {
                        GroupName = artist,
                        JumpNode = string.IsNullOrEmpty(artist) ? "" : artist[0].ToString()
                    };
                    g.Albums = group.OrderBy(c => c.Artist).ToArray();
                    _groups.Add(g);
                }
            }
            else if (type == AlbumDetailsOrderType.Year)
            {
                var groups = Albums.GroupBy(a => a.Year);
                double step_width = 100 / Math.Max(1, groups.Count());
                double step = 0;

                foreach (var group in groups)
                {
                    step += step_width;
                    MainWindow.SetProgress(step);
                    string year = group.First().Year.ToString();
                    AlbumDetailGroup g = new AlbumDetailGroup()
                    {
                        GroupName = year,
                        JumpNode = year
                    };
                    g.Albums = group.OrderBy(c => c.Year).ToArray();
                    _groups.Add(g);
                }
            }
            else if (type == AlbumDetailsOrderType.All)
            {
                double step_width = 100 / Math.Max(1, Albums.Count);

                AlbumDetailGroup g = new AlbumDetailGroup() { GroupName = string.Empty };
                g.Albums = Albums.ToArray();
                _groups.Add(g);
            }
            
            AlbumGroups = _groups;

            MainWindow.SetProgress(0);
        }

        public void OrderBy(AlbumDetailsOrderType ot)
        {
            if (MainWindow.Instance.ViewMode != ViewMode.Albums)
                return;

            switch (ot)
            {
                case AlbumDetailsOrderType.Album:
                    Albums = Library.Instance.Current.Albums.Where(a => a.IsVisible).OrderBy(o => o.Album).ToList();
                    break;
                case AlbumDetailsOrderType.Artist:
                    Albums = Library.Instance.Current.Albums.Where(a => a.IsVisible).OrderBy(o => o.Artist).ToList();
                    break;
                case AlbumDetailsOrderType.Year:
                    Albums = Library.Instance.Current.Albums.Where(a => a.IsVisible).OrderByDescending(o => o.Year).ToList();
                    break;
                case AlbumDetailsOrderType.All:
                    Albums = Library.Instance.Current.Albums.Where(a => a.IsVisible).OrderBy(o => o.Album).ToList();
                    break;
            }

            ViewType = ot;

            getGroups(ot);

        }
    }
}

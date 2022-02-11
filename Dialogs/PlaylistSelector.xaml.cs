using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Classes;
using ViewModels;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Dialogs
{
    public class PlaylistDefinition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private bool _autoSelect = false;
        public bool AutoSelect
        {
            get => _autoSelect;
            set
            {
                _autoSelect = value;
                Raise(nameof(AutoSelect));
            }
        }

        private string _name = "Enter Your Playlist Name Here";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Raise(nameof(Name));
            }
        }

        private string _path = "";
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                Raise(nameof(Path));
            }
        }
    }

    public partial class PlaylistSelector : Window, INotifyPropertyChanged
    {
        string libfile = "Playlists.xml";

        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        PlaylistViewModel viewModel = null;
        public PlaylistDefinition DialogSelection = null;

        public PlaylistSelector(PlaylistViewModel viewModel)
        {
            this.viewModel = viewModel;
            this.DataContext = this;
            InitializeComponent();

            readPlaylistXML();
        }

        void readPlaylistXML()
        {
            if (!File.Exists(libfile))
                return;

            XDocument doc = XDocument.Load(libfile);
            var authors = doc.Descendants("Playlist");
            foreach (var author in authors)
            {
                if (author.Attribute("Name") == null ||
                    author.Attribute("Path") == null ||
                    author.Attribute("Select") == null)
                    continue;

                string name = author.Attribute("Name").Value;
                string path = author.Attribute("Path").Value;
                bool autoSelect = false;
                bool.TryParse(author.Attribute("Select").Value, out autoSelect);

                PlaylistDefs.Add(new PlaylistDefinition() { Name = name, Path = path, AutoSelect = autoSelect });
            }

            SelectedDefinition = PlaylistDefs.FirstOrDefault(x => x.AutoSelect);

            reloadPlaylists();
        }

        string sanitize(string str)
        {
            char[] _c = new char[] { '/', '\\', '"', '\t', '\n', '\r' };
            foreach (char c in _c)
                str = str.Replace(c, '_');
            return str;
        }

        public void SavePlaylistXML()
        {
            string doc = "<xml>\n\t<Playlists>\n";
            foreach (PlaylistDefinition def in PlaylistDefs)
                doc += "\t\t<Playlist Name=\"" + sanitize(def.Name) + "\" Path=\"" + def.Path + "\" Select=\"" + def.AutoSelect.ToString() + "\"/>\n";
            doc += "\t</Playlists>\n</xml>";

            File.WriteAllText(libfile, doc);
        }

        void reloadPlaylists()
        {
            ListLibraries.ItemsSource = null;
            ListLibraries.ItemsSource = PlaylistDefs.OrderBy(l => l.Name);
            SavePlaylistXML();
        }

        PlaylistDefinition _currentDefinition = new PlaylistDefinition();
        public PlaylistDefinition CurrentDefinition
        {
            get =>  _currentDefinition;
            set
            {
                _currentDefinition = value;
                Raise(nameof(CurrentDefinition));
            }
        }

        List<PlaylistDefinition> _playlistDefs = new List<PlaylistDefinition>();

        public List<PlaylistDefinition> PlaylistDefs
        {
            get => _playlistDefs;
            set
            {
                _playlistDefs = value;
                Raise(nameof(PlaylistDefs));
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonSaveFileDialog();
            dialog.Title = "Please select a location for your playlist file";
            string plpath = System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Playlists");
            if (!Directory.Exists(plpath))
                Directory.CreateDirectory(plpath);
            dialog.DefaultDirectory = plpath;
            string defaultname = _currentDefinition.Name;
            if (string.IsNullOrEmpty(defaultname))
                defaultname = "Playlist_1.pls";
            dialog.DefaultFileName = defaultname;
            dialog.DefaultExtension = "pls";
            dialog.InitialDirectory = plpath;
            dialog.Filters.Add(new CommonFileDialogFilter("Playlist File", "*.pls"));

            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;
                int idx = path.LastIndexOf('.');
                if (idx > 0)
                    path = path.Substring(0, idx);
                path += ".pls";
                _currentDefinition.Path = dialog.FileName;
            }

            BringIntoView();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_currentDefinition.Name))
            {
                string plpath = System.IO.Path.Combine(
               System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Playlists");
                if (!Directory.Exists(plpath))
                    Directory.CreateDirectory(plpath);

                string name = sanitize(_currentDefinition.Name);
                string storage = System.IO.Path.Combine(plpath, name) + ".pls";
                _currentDefinition.Path = storage;

                PlaylistDefinition pDef = PlaylistDefs.FirstOrDefault(p => p.Name == _currentDefinition.Name);
                if (pDef != null)
                {
                    pDef.Path = _currentDefinition.Path;
                }
                else
                {
                    pDef = new PlaylistDefinition()
                    {
                        Name = name,
                        Path = _currentDefinition.Path
                    };
                    PlaylistDefs.Add(pDef);
                }

                serialize(pDef);

                _currentDefinition.Name = "";
                _currentDefinition.Path = "";

                Raise(nameof(PlaylistDefs));

                reloadPlaylists();
            }
        }

        void serialize(PlaylistDefinition def) => PlaylistSerializer.Serialize(def.Path, viewModel.Tracks);

        PlaylistDefinition _selectedDefinition = null;
        public PlaylistDefinition SelectedDefinition
        {
            get => _selectedDefinition;
            set
            {
                _selectedDefinition = value;
                Raise(nameof(SelectedDefinition));
            }
        }
        private void ListLibraries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedItem == null)
                return;

            PlaylistDefinition def = (sender as ListView).SelectedItem as PlaylistDefinition;
            _currentDefinition.Name = def.Name;
            _currentDefinition.Path = def.Path;

            SelectedDefinition = def;
        }

        private void ListLibraries_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete)
                return;

            if ((sender as ListView).SelectedItem == null)
                return;

            PlaylistDefinition def = (sender as ListView).SelectedItem as PlaylistDefinition;

            if (File.Exists(def.Path))
            {
                try
                {
                    File.Delete(def.Path);
                }
                catch { }
            }

            PlaylistDefs.Remove(def);

            reloadPlaylists();
        }

        private void ListLibraries_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SavePlaylistXML();

            if ((sender as ListView).SelectedItem == null)
                return;

            PlaylistDefinition def = (sender as ListView).SelectedItem as PlaylistDefinition;
            SelectedDefinition = def;
            DialogSelection = def;

            PlaylistSerializer.Load(viewModel, def.Path);

            Close();
        }


        private void cbDefault_Click(object sender, RoutedEventArgs e)
        {
            if (cbDefault.IsChecked == true)
            {
                if (SelectedDefinition == null)
                    return;

                PlaylistDefinition def = SelectedDefinition;
                foreach (PlaylistDefinition _d in PlaylistDefs)
                {
                    if (_d != def) _d.AutoSelect = false;
                    else _d.AutoSelect = true;
                }
            }
            else
                foreach (PlaylistDefinition _d in PlaylistDefs)
                    _d.AutoSelect = false;

            SavePlaylistXML();
        }
    }
}

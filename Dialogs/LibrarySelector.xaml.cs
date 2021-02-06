using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Classes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MpFree4k.Dialogs
{
    public class MediaLibraryDefinition : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private bool _autoSelect = false;
        public bool AutoSelect
        {
            get { return _autoSelect; }
            set
            {
                _autoSelect = value;
                OnPropertyChanged("AutoSelect");
            }
        }

        private string _name = "Enter Your Library's Name Here";
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private string _path = "";
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged("Path");
            }
        }
    }

    /// <summary>
    /// Interaktionslogik für LibrarySelector.xaml
    /// </summary>
    public partial class LibrarySelector : Window, INotifyPropertyChanged
    {
        string libfile = "MediaLibraries.xml";

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        public LibrarySelector()
        {
            this.DataContext = this;
            InitializeComponent();

            readLibs();
        }

        void readLibs()
        {
            if (!File.Exists(libfile))
                return;

            XDocument doc = XDocument.Load(libfile);
            var authors = doc.Descendants("MediaLibrary");
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

                LibDefs.Add(new MediaLibraryDefinition() { Name = name, Path = path, AutoSelect = autoSelect });
            }

            SelectedLib = LibDefs.FirstOrDefault(x => x.AutoSelect);

            reloadLibs();
        }

        string sanitize(string str)
        {
            char[] _c = new char[] { '/', '\\', '"', '\t', '\n', '\r'};
            foreach (char c in _c)
                str = str.Replace(c, '_');
            return str;
        }

        void saveLibs()
        {
            string doc = "<xml>\n\t<MediaLibraries>\n";
            foreach (MediaLibraryDefinition def in LibDefs)
            {
                doc += "\t\t<MediaLibrary Name=\"" + sanitize(def.Name) + "\" Path=\"" + def.Path + "\" Select=\"" + def.AutoSelect.ToString() + "\"/>\n";
            }
            doc += "\t</MediaLibraries>\n</xml>";

            File.WriteAllText(libfile, doc);
        }

        void reloadLibs()
        {
            ListLibraries.ItemsSource = null;
            ListLibraries.ItemsSource = LibDefs.OrderBy(l => l.Name);

            saveLibs();
        }

        private MediaLibraryDefinition _currentDefinition = new MediaLibraryDefinition();
        public MediaLibraryDefinition CurrentDefinition
        {
            get { return _currentDefinition; }
            set
            {
                _currentDefinition = value;
                OnPropertyChanged("CurrentDefinition");
            }
        }

        private List<MediaLibraryDefinition> _libdefs = new List<MediaLibraryDefinition>()
        {
            //new MediaLibraryDefinition() { Name = "Default Library - Complete", Path = @"C:\Media\mp3"},
            //new MediaLibraryDefinition() { Name = "Compilations", Path = @"C:\Media\mp3\_Compilations"}
        };
        public List<MediaLibraryDefinition> LibDefs
        {
            get { return _libdefs; }
            set
            {
                _libdefs = value;
                OnPropertyChanged("LibDefs");
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = false;

            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "please select your library location";

            CommonFileDialogResult result = dialog.ShowDialog(this);

            if (result == CommonFileDialogResult.Ok)
                _currentDefinition.Path = dialog.FileName;

            this.Topmost = true;
            this.BringIntoView();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_currentDefinition.Name) &&
                !string.IsNullOrWhiteSpace(_currentDefinition.Path))
            {
                LibDefs.Add(new MediaLibraryDefinition()
                {
                    Name = sanitize(_currentDefinition.Name),
                    Path = _currentDefinition.Path
                });
                _currentDefinition.Name = "";
                _currentDefinition.Path = "";

                OnPropertyChanged("LibDefs");

                reloadLibs();
            }
        }

        private MediaLibraryDefinition _selectedLib = null;
        public MediaLibraryDefinition SelectedLib
        {
            get { return _selectedLib;  }
            set
            {
                _selectedLib = value;
                OnPropertyChanged("SelectedLib");
            }
        }
        private void ListLibraries_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedItem == null)
                return;

            MediaLibraryDefinition def = (sender as ListView).SelectedItem as MediaLibraryDefinition;
            _currentDefinition.Name = def.Name;
            _currentDefinition.Path = def.Path;

            SelectedLib = def;
        }

        private void ListLibraries_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete)
                return;

            if ((sender as ListView).SelectedItem == null)
                return;

            MediaLibraryDefinition def = (sender as ListView).SelectedItem as MediaLibraryDefinition;
            LibDefs.Remove(def);

            reloadLibs();
        }

        public MediaLibraryDefinition DialogSelection = null;
        private void ListLibraries_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            saveLibs();

            if ((sender as ListView).SelectedItem == null)
                return;

            MediaLibraryDefinition def = (sender as ListView).SelectedItem as MediaLibraryDefinition;

            SelectedLib = def;
            DialogSelection = def;

            Config.MediaHasChanged = true;
            this.Close();
        }

        MediaLibraryDefinition CheckedLib = null;


        private void cbDefault_Click(object sender, RoutedEventArgs e)
        {
            if (cbDefault.IsChecked == true)
            {
                if (SelectedLib == null)
                    return;

                MediaLibraryDefinition def = SelectedLib;
                foreach (MediaLibraryDefinition _d in LibDefs)
                {
                    if (_d != def)
                        _d.AutoSelect = false;
                    else
                    {
                        _d.AutoSelect = true;
                    }
                }
            }
            else
            {
                foreach (MediaLibraryDefinition _d in LibDefs)
                {
                    _d.AutoSelect = false;
                }
            }

            saveLibs();
        }
    }
}

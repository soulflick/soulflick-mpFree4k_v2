using MpFree4k.Classes;
using MpFree4k.Dialogs;
using System;
using System.ComponentModel;

namespace MpFree4k.Layers
{
    public class Library : INotifyPropertyChanged
    {
        public SQLiteConnector connector = null;
    
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { return; };

        public void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private MediaLibrary _current = null;
        public MediaLibrary Current
        {
            get { return _current; }
            set
            {
                _current = value;
                OnPropertyChanged("Current");
            }
        }

        public static Library _singleton = null;
        public Library()
        {
            _singleton = this;

            connector = new SQLiteConnector();
            connector.init();

            Current = new MediaLibrary();

            LibrarySelector sel = new LibrarySelector();
            if (sel.SelectedLib != null)
            {
                Current.Name = sel.SelectedLib.Name;
                Current.LibPath = sel.SelectedLib.Path;
            }
            else
            {
                sel.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                sel.ShowDialog();
                if (sel.DialogSelection != null)
                {
                    Current.Name = sel.DialogSelection.Name;
                    Current.LibPath = sel.DialogSelection.Path;
                }
            }
        }

        public void Load()
        {
            if (!string.IsNullOrEmpty(Current.LibPath))
            Current.Load();
        }

        public static void LoadFrom(MediaLibraryDefinition def)
        {
            //_singleton.Current = new MediaLibrary();
            _singleton.Current.Reset();
            _singleton.Current.Name = def.Name;
            _singleton.Current.LibPath = def.Path;
            _singleton.Load();
        }
    }
}

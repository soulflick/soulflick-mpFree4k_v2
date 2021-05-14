using MpFree4k.Classes;
using MpFree4k.Dialogs;
using System.ComponentModel;

namespace MpFree4k.Layers
{
    public class Library : INotifyPropertyChanged
    {
        public SQLiteConnector connector = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private MediaLibrary _current = null;
        public MediaLibrary Current
        {
            get => _current;
            set
            {
                _current = value;
                OnPropertyChanged("Current");
            }
        }

        public static Library Instance = null;
        public Library()
        {
            Instance = this;

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
            Instance.Current.Reset();
            Instance.Current.Name = def.Name;
            Instance.Current.LibPath = def.Path;
            Instance.Load();
        }
    }
}

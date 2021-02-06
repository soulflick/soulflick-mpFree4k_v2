using Classes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MpFree4k.Dialogs
{
    public partial class TracksEditor : Window, INotifyPropertyChanged
    {
        public TracksEditor(FileViewInfo[] infos)
        {
            FileInfos = new ObservableCollection<FileViewInfo>(infos);
            InitializeComponent();
            Decide();
            this.DataContext = this;
            
        }

        void Decide()
        {
            if (FileInfos.Count == 1)
            {
                MaxWidth = MinWidth = Width = ColumnDetails.Width.Value + 20;
                ColumnList.Width = new GridLength(0);
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private ObservableCollection<FileViewInfo> fileInfos = null;

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<FileViewInfo> FileInfos
        {
            get => fileInfos;
            set
            {
                fileInfos = value;
                RaisePropertyChanged(nameof(FileInfos));
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileViewInfo info = (sender as ListView).SelectedItem as FileViewInfo;
            Editor.CurrentTag = info;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOkay_Click(object sender, RoutedEventArgs e)
        {
            FileInfos.ToList().ForEach(file => file.save());
            this.Close();
        }
    }
}

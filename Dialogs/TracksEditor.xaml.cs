using Classes;
using Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ViewModels;

namespace Dialogs
{
    public partial class TracksEditor : Window, INotifyPropertyChanged
    {
        public TracksEditor(FileViewInfo[] infos)
        {
            FileInfos = new ObservableCollection<FileViewInfo>(infos.OrderBy(x => x.Path));
            InitializeComponent();
            Decide();
            DataContext = this;            
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
        public void Raise(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        public ObservableCollection<FileViewInfo> FileInfos
        {
            get => fileInfos;
            set
            {
                fileInfos = value;
                Raise(nameof(FileInfos));
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedItems == null)
            {
                Editor.AllFiles = new List<FileViewInfo>();
            }
            else
            {
                Editor.AllFiles = new List<FileViewInfo>((sender as ListView).SelectedItems.Cast<FileViewInfo>().ToArray());
            }
            FileViewInfo info = (sender as ListView).SelectedItem as FileViewInfo;
            Editor.CurrentTag = info;

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Editor.CancelEvents();
            FileInfos.ToList().ForEach(file =>
            {
                file.ReadMp3Fields();
            });
            Close();
        }

        private void btnOkay_Click(object sender, RoutedEventArgs e)
        {
            Editor.CancelEvents();
            FileInfos.ToList().ForEach(file =>
            {
                file.save();
                file.CreateFileHandle();
                PlaylistViewModel.Instance.UpdateFile(file);
            });
            Close();
        }
    }
}

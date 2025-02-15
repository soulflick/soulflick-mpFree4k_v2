using Classes;
using Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using ViewModels;

namespace Dialogs
{
    public partial class TracksEditor : Window, INotifyPropertyChanged
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x00080000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public TracksEditor(FileViewInfo[] infos)
        {
            Loaded += new RoutedEventHandler(Window_Loaded);
            FileInfos = new ObservableCollection<FileViewInfo>(infos.OrderBy(x => x.Path));
            InitializeComponent();
            Decide();
            DataContext = this;            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLongPtr(hwnd, GWL_STYLE) & ~WS_SYSMENU);
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
                Editor.CurrentTag = (sender as ListView).SelectedItem as FileViewInfo;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Editor.CancelEvents();
            FileInfos.ToList().ForEach(file => file.ReadMp3Fields());
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

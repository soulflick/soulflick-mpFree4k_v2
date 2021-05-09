using MpFree4k.ViewModels;
using System.Windows;
using System.Windows.Controls;
using WPFEqualizer;

namespace MpFree4k.Dialogs
{
    public partial class EqualizerWindow : Window
    {
        private EqualizerWindowViewModel ViewModel { get; } = new EqualizerWindowViewModel();
        public EqualizerWindow()
        {
            InitializeComponent();

            DataContext = ViewModel;            
        }

        private void PresetChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.Presets == null)
                return;

            var index = (sender as ComboBox).SelectedIndex;
            EQControl.Set(ViewModel.GetEQFrom(ViewModel.Presets[index].Path));
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            EQControl.Reset();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Save();
        }
    }
}

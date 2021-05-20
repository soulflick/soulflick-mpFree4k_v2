using ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Dialogs
{
    public partial class EqualizerWindow : Window
    {
        public static int SelectedPreset;
        private bool presets_clicked;

        private EqualizerWindowViewModel ViewModel { get; } = new EqualizerWindowViewModel();
        public EqualizerWindow()
        {
            InitializeComponent();

            DataContext = ViewModel;

            Loaded += EqualizerWindow_Loaded;
        }

        private void EqualizerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            presets_clicked = false;
            cmbPresets.SelectedIndex = SelectedPreset;
        }

        private void PresetChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.Presets == null)
                return;

            var index = (sender as ComboBox).SelectedIndex;
            EQControl.Set(ViewModel.GetEQFrom(ViewModel.Presets[index].Path));

            if (presets_clicked)
                SelectedPreset = index;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e) => EQControl.Reset();

        private void btnSave_Click(object sender, RoutedEventArgs e) => ViewModel.Save();

        private void cmbPresets_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => presets_clicked = true;
    }
}

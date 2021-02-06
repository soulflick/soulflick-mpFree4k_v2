using System.Windows;
using System.Windows.Controls;

namespace MpFree4k.Dialogs
{
    public partial class EqualizerWindow : Window
    {
        public EqualizerWindow()
        {
            InitializeComponent();
            Loaded += EqualizerWindow_Loaded;
        }

        private void EqualizerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = EQControl.ActualWidth +  60;
        }

        private void PresetChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            EQControl.Reset();
        }
    }
}

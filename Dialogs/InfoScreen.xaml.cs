using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Dialogs
{
    public partial class InfoScreen : Window
    {
        public InfoScreen() => InitializeComponent();

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}

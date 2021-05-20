using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace MpFree4k.Windows
{
    public partial class SmallView : Window
    {
        public SmallView() => InitializeComponent();

        private void _This_Loaded(object sender, RoutedEventArgs e)
        {
            double left = (SystemParameters.PrimaryScreenWidth - ActualWidth) / 2;
            Top = 0;
            Left = left;
        }

        public bool _closed = false;
        private void _This_Closing(object sender, EventArgs e)
        {
            _closed = true;
        }

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}

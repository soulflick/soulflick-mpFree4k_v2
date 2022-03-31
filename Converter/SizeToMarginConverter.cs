using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mpfree4k.Converters
{
    public class SizeToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double pixels = (double)value;
            return new GridLength(pixels);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}

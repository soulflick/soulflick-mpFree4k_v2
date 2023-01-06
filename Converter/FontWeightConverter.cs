using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mpfree4k.Converters
{
    public class FontWeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return FontWeights.Normal;

            string title = values[0]?.ToString();
            string fieldtitle = values[1]?.ToString();

            return (title == fieldtitle) ? FontWeights.Bold : FontWeights.Normal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class TrackColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2)
                return FontWeights.Normal;

            string title = values[0]?.ToString();
            string fieldtitle = values[1]?.ToString();

            return (title == fieldtitle) ? Brushes.White : Brushes.Gainsboro;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

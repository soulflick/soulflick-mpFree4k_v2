using System;
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

            if (int.TryParse(values[0].ToString(), out int track) &&
                int.TryParse(values[1].ToString(), out int position) &&
                track == position)
                return FontWeights.Bold;

            return FontWeights.Normal;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

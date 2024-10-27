using System;
using System.Globalization;
using System.Windows.Data;

namespace Mpfree4k.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public class PathNameConverter : IValueConverter
    {
        public object Convert(object value, Type t, object parameter, CultureInfo culture)
        {
            string fileName = value as string;
            return System.IO.Path.GetDirectoryName(fileName);
        }

        public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

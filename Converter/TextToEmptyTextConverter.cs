using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace Mpfree4k.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class TextToEmptyTextConverter: IValueConverter
    {
        public object Convert(object value, Type t, object parameter, CultureInfo culture)
        {
            double dvalue = 0;
            if (double.TryParse(value.ToString(), out dvalue))
            {
                if (dvalue <= 0) return string.Empty;
                else
                {
                    if (parameter == null)
                        return value;
                    else
                        return parameter;
                }
            }
            else return value;
        }

        public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

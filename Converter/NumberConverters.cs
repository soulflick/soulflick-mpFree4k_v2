using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpFree4k.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    public class EmptyStringToNotificationConverter : IValueConverter
    {
        public object Convert(object value, Type t, object parameter, CultureInfo culture)
        {
            string msg = parameter is string ? parameter.ToString() : "[empty]";
            if (value is string && string.IsNullOrEmpty(value.ToString()))
                return msg;
            else
                return value;
        }

        public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
        {
            return value;
        }

    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object parameter, CultureInfo culture)
        {
            if (value is string && !string.IsNullOrEmpty(value.ToString()))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
        {
            return value;
        }

    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type t, object parameter, CultureInfo culture)
        {
            if (value is bool)
                return ((bool)value) ? 1 : 0.5;
            return 1;
        }

        public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class AddConverter : IValueConverter
    {
        public object Convert(object value, Type t, object parameter, CultureInfo culture)
        {
            if (!(value is double))
                return value;


            double num = 0;
            if (!double.TryParse(parameter.ToString(), out num))
                return value;

            double val = (double)value;
            return val + num;
        }

        public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
        {
            return value;
        }
    }


    [ValueConversion(typeof(BitmapImage), typeof(BitmapImage))]
    public class NoImageToDefault : IValueConverter
    {

        public static BitmapImage DefaultAlbumImage = null;

        public object Convert(object value, Type t, object parameter, CultureInfo culture)
        {
            if (DefaultAlbumImage == null)
            {
                string uri = @"pack://application:,,,/" + "MpFree4k" +
                ";component/" + "Images/no_album_cover.jpg";

                DefaultAlbumImage = new System.Windows.Media.Imaging.BitmapImage(
                new System.Uri(uri, System.UriKind.Absolute));
            }

            if (value == null)
                return DefaultAlbumImage;

            else return value;
        }

        public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class GetSizeConverter : IValueConverter
    {
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        public object Convert(object value, Type t, object parameter, CultureInfo culture)
        {
            if (!(value is FrameworkElement))
                return 16;

            double param = 0;
            if (parameter is double)
                param = (double)parameter;

            if (value is ListView)
            {

                ListView view = value as ListView;
                double wid = view.ActualWidth;

                ScrollViewer scrollview = FindVisualChild<ScrollViewer>(view);
                if (scrollview != null)
                {
                    Visibility verticalVisibility = scrollview.ComputedVerticalScrollBarVisibility;
                    if (verticalVisibility != Visibility.Collapsed)
                    {
                        double substract = SystemParameters.VerticalScrollBarWidth;
                        wid -= substract;
                    }
                }
                return wid + param;
            }
            return value;
        }

        public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

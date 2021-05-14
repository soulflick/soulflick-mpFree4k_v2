using System.Windows;
using System.Windows.Media;

namespace MpFree4k.Classes
{
    public static class VisualTreeHelperLocal
    {
        public static  childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
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

        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            //for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            //{
            //    var child = VisualTreeHelper.GetChild(depObj, i);

            //    var result = (child as T) ?? GetChildOfType<T>(child);
            //    if (result != null) return result;
            //}

            return null;
        }
    }
}

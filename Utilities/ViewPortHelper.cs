using System;
using System.Reflection;
using System.Windows.Controls;

namespace Utilities
{
    public static class ViewportHelper
    {
        public static bool IsInViewport(Control item)
        {
            if (item == null) return false;
            ItemsControl itemsControl = null;
            if (item is ListBoxItem)
            {
                itemsControl = ItemsControl.ItemsControlFromItemContainer(item) as ListBox;
            }
            else
            {
                throw new NotSupportedException(item.GetType().Name);
            }

            ScrollViewer scrollViewer = Utilities.VisualTreeHelper.GetVisualChild<ScrollViewer, ItemsControl>(itemsControl);
            ScrollContentPresenter scrollContentPresenter = (ScrollContentPresenter)scrollViewer.Template.FindName("PART_ScrollContentPresenter", scrollViewer);
            MethodInfo isInViewportMethod = scrollViewer.GetType().GetMethod("IsInViewport", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)isInViewportMethod.Invoke(scrollViewer, new object[] { scrollContentPresenter, item });
        }
    }
}

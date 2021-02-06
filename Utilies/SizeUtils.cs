namespace WPFEqualizer.Utils
{
    public static class SizeUtils
    {
        public static bool IsRealSize(this System.Drawing.Size size)
        {
            return !double.IsNaN(size.Width) && !double.IsNaN(size.Height) && size.Width > 0 && size.Height > 0;
        }
    }
}

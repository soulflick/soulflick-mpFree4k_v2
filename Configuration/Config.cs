namespace Configuration
{
    public static class Config
    {
        public static string[] media_extensions = new string[] { ".mp3", ".m4a", ".mpeg", ".m2a", ".mp2", ".mp1", ".wav" };
        public static int update_timeout = 950;
        public static int drag_pixel = 8;
        public static bool MediaHasChanged = false;
        public static bool OpenSmallWindowWhenMinimized = true;
    }
}

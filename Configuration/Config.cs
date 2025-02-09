using Mpfree4k.Enums;

namespace Configuration
{
    public static class Config
    {
        public static string[] media_extensions = new string[] { ".mp3", ".ogg", ".m4a", ".mpeg", ".m2a", ".mp2", ".mp1", ".wav" };
        public static int update_timeout = 25;
        public static int drag_pixel = 8;
        public static bool MediaHasChanged = false;
        public static ScalingStrategy EQScalingStrategy = ScalingStrategy.Linear;
    }
}

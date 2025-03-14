using Mpfree4k.Enums;

namespace Configuration
{
    public static class UserConfig
    {
        private static bool _showFullAlbum = true;
        public static bool ShowFullAlbum 
        { 
            get => _showFullAlbum;
            set => _showFullAlbum = value;
        }

        private static bool _autoSavePlaylist = true;
        public static bool AutoSavePlaylist
        {
            get => _autoSavePlaylist;
            set=> _autoSavePlaylist = value;
        }

        private static bool _rememberSelectedAlbum = true;
        public static bool RememberSelectedAlbums
        {
            get => _rememberSelectedAlbum;
            set => _rememberSelectedAlbum = value;
        }

        private static bool _showPathInPlaylist = false;
        public static bool ShowPathInPlaylist
        {
            get => _showPathInPlaylist;
            set => _showPathInPlaylist = value;
        }

        private static bool _showPathInLibrary = false;
        public static bool ShowPathInLibrary
        {
            get => _showPathInLibrary;
            set => _showPathInLibrary = value;
        }

        private static bool _excludeBrokenFiles = false;
        public static bool ExcludeBrokenFiles
        {
            get => _excludeBrokenFiles;
            set => _excludeBrokenFiles = value;
        }

        public static PluginTypes PluginType = PluginTypes.CSCore;
        public static SkinColors Skin { get; set; } = SkinColors.Black_Smooth;
        public static FontSize FontSize = FontSize.Normal;
        public static ControlSize ControlSize = ControlSize.Bigger;
        public static ArtistViewType ArtistViewType = ArtistViewType.Plain;
        public static AlbumViewType AlbumViewType = AlbumViewType.Detail;
        public static TrackViewType TrackViewType = TrackViewType.List;
        public static PaddingType PaddingType = PaddingType.One;
        public static int NumberRecentAlbums = 15;
        public static int NumberRecentTracks = 800;

    }
}

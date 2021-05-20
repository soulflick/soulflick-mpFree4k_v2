namespace Mpfree4k.Enums
{
    public enum ViewMode
    {
        Details,
        Table,
        Albums,
        Favourites
    }

    public enum MediaLevel
    {
        All,
        Artists,
        Albums,
        Tracks
    }

    public enum ScalingStrategy
    {
        Decibel,
        Linear,
        Sqrt
    }

    public enum SelectedControl
    {
        None,
        Albums,
        Tracks
    }

    public enum RemainingMode
    {
        Elapsed,
        Remaining
    }

    public enum Playmode
    {
        Play,
        Unplay
    }

    public enum GraphType
    {
        Bar,
        Line,
        Band
    }

    public enum TabOrder
    {
        Detail = 0,
        Albums = 1,
        Tracks = 2,
        Favourites = 3
    }

    public enum ControlSize
    {
        Small = 24,
        Normal = 34,
        Bigger = 48,
        Biggest = 64,
        Huge = 80
    }

    public enum RepeatMode
    {
        Once,
        RepeatOne,
        GoThrough,
        Loop,
        Shuffle
    }

    public enum SkinColors
    {
        White = 0,
        Gray = 1,
        Dark = 2,
        Blue = 3,
        Black_Smooth = 4
    }

    public enum PluginTypes
    {
        WMPLib,
        CSCore
    }

    public enum FontSize
    {
        Smallest = 0,
        Smaller,
        Small,
        Medium,
        Normal,
        Big,
        Bigger,
        Biggest,
        Huge,
        Gonzo
    }

    public enum TrackOrderType
    {
        Standard,
        TrackName,
        Album,
        Artist,
        Year,
        Genre
    }

    public enum AlbumOrderType
    {
        Album,
        Artist,
        Year,
        Genre,
        Tracks
    }

    public enum AlbumDetailsOrderType
    {
        Album,
        Artist,
        Year,
        All
    }

    public enum ArtistViewType
    {
        List = 0,
        BigTile = 1,
        AlbumTile = 2,
        Plain = 3
    }

    public enum AlbumViewType
    {
        List = 0,
        Detail = 1,
        Plain = 2
    }

    public enum TrackViewType
    {
        Plain,
        List,
        Details
    }

    public enum ArtistOrderType
    {
        Artist,
        Year,
        Genre,
        MostContent
    }
}

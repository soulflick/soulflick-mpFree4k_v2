using Classes;
using Models;
using System;

namespace Classes
{
    public class PlaylistHelpers
    {
        public static string getDurationFromLabel(string str)
        {
            if (str.IndexOf("-") < 0) return string.Empty;
            string dur = str.Substring(0, str.IndexOf("-") - 1);
            return dur;
        }

        static string solveDuration(string durationString)
        {
            string duration = durationString;
            if (!durationString.Contains(":")) return $"00:{durationString}";
            string prefix = durationString.Substring(0, duration.IndexOf(":"));
            if (prefix == "00") duration = duration.Substring(duration.IndexOf(":") + 1);

            if (duration.IndexOf(':') < 0)
                duration = $"00:{duration}";

            return duration;
        }

        public static PlaylistInfo CreateFromFileViewInfo(FileViewInfo infoItm)
        {
            PlaylistInfo plItm = new PlaylistInfo();

            plItm.Path = infoItm.Path;
            plItm.Duration = solveDuration(infoItm.Mp3Fields.Duration);
            plItm.DurationValue = infoItm.Mp3Fields.DurationValue;
            plItm.TrackName = string.Empty;

            if (!string.IsNullOrEmpty(infoItm.Mp3Fields.Title) && !string.IsNullOrEmpty(infoItm.Mp3Fields.AlbumArtists))
                plItm.TrackName += infoItm.Mp3Fields.Title + " - " + String.Join(" ", infoItm.Mp3Fields.AlbumArtists);
            else plItm.TrackName += infoItm.FileName;
            plItm.TrackLabel = plItm.TrackName;

            plItm.Title = infoItm.Mp3Fields.Title;
            plItm.Artists = infoItm.Mp3Fields.AlbumArtists;
            plItm.Album = infoItm.Mp3Fields.Album;
            plItm.Year = infoItm.Mp3Fields.Year.ToString();

            SetToolTip(plItm);

            return plItm;
        }

        public static void CreateFromFileViewInfo(PlaylistInfo plItm, FileViewInfo infoItm)
        {
            plItm.Path = infoItm.Path;
            plItm.Duration = solveDuration(infoItm.Mp3Fields.Duration);
            plItm.DurationValue = infoItm.Mp3Fields.DurationValue;
            plItm.TrackName = string.Empty;

            if (!string.IsNullOrEmpty(infoItm.Mp3Fields.Title) && !string.IsNullOrEmpty(infoItm.Mp3Fields.AlbumArtists))
                plItm.TrackName += infoItm.Mp3Fields.Title + " - " + String.Join(" ", infoItm.Mp3Fields.AlbumArtists);
            else plItm.TrackName += infoItm.FileName;
            plItm.TrackLabel = plItm.TrackName;

            plItm.Title = infoItm.Mp3Fields.Title;
            plItm.Artists = infoItm.Mp3Fields.AlbumArtists;
            plItm.Album = infoItm.Mp3Fields.Album;
            plItm.Year = infoItm.Mp3Fields.Year.ToString();

            SetToolTip(plItm);
        }

        static void SetToolTip(PlaylistInfo itm)
        {
            itm.ToolTip = itm.Title + "\n"
                + itm.Artists + "\n"
                + itm.Album + " (" + itm.Year + ") " + "\n"
                + itm.Path;

            itm.ToolTip =
                itm.Title + "\n" +
                "Artist: " + itm.Artists + "\n" +
                "Album: " + itm.Album + "\n" +
                "Track: " + itm.Track + "\n" +
                "Year: " + itm.Year.ToString() + "\n" +
                "[" + itm.Path + "]";
        }

        public static PlaylistInfo CreateFromMediaItem(FileViewInfo infoItm)
        {
            PlaylistInfo itm = new PlaylistInfo();
            CreateFromMediaItem(itm, infoItm);
            return itm;
        }

        public static void CreateFromMediaItem(PlaylistInfo itm, FileViewInfo infoItm)
        {
            if (infoItm == null)
                return;

            itm.Duration = solveDuration(infoItm.Mp3Fields.Duration);
            itm.DurationValue = infoItm.Mp3Fields.DurationValue;
            itm.TrackName = string.Empty;

            itm.Path = infoItm.Path;
            if (string.IsNullOrEmpty(infoItm.Mp3Fields.Title) && string.IsNullOrEmpty(infoItm.Mp3Fields.AlbumArtists))
                itm.TrackName += infoItm.FileName;
            else
                itm.TrackName += infoItm.Mp3Fields.Title + " - " + infoItm.Mp3Fields.AlbumArtists;

            itm.Track = infoItm.Mp3Fields.Track;
            itm.Title = infoItm.Mp3Fields.Title;
            itm.Artists = infoItm.Mp3Fields.AlbumArtists;
            itm.Album = infoItm.Mp3Fields.Album;
            itm.Year = infoItm.Mp3Fields.Year.ToString();
            itm.TrackLabel = itm.TrackName;

            SetToolTip(itm);
        }

        public static string CreateUniqueID() => Guid.NewGuid().ToString("N");

    }
}

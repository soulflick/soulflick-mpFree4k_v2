using Classes;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public static class LibraryUtils
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static FileViewInfo[] GetAlbumItems(FileViewInfo info, IEnumerable<FileViewInfo> collection)
        {
            var protoList = collection.Where(cItem => cItem.Mp3Fields.Artists == info.Mp3Fields.Artists && 
                                             cItem.Mp3Fields.Album  == info.Mp3Fields.Album);

            return protoList.DistinctBy(p => p.Title).OrderBy(o => o.Mp3Fields.Track).ToArray();
        }

        public static IEnumerable<PlaylistItem> GetItems(string[] tracks)
        {
            foreach (string track in tracks)
            {
                FileViewInfo fi = new FileViewInfo(track);
                PlaylistItem plitm = new PlaylistItem();
                PlaylistHelpers.CreateFromMediaItem(plitm, fi);
                yield return plitm;
            }
        }
        public static FileViewInfo[] GetInfoItems(AlbumItem album)
        {
            return album.Tracks.ToList().Select(t => new FileViewInfo(t)).ToArray();
        }

        public static FileViewInfo[] GetInfoItems(SimpleAlbumItem album)
        {
            return album.Tracks.ToList().Select(t => new FileViewInfo(t)).ToArray();
        }

        public static FileViewInfo[] GetInfoItems(PlaylistItem[] items)
        {
            return items.ToList().Select(i => new FileViewInfo(i.Path)).ToArray();
        }

        public static PlaylistItem[] GetTracks(AlbumItem album)
        {
            if (album == null || album.Tracks == null || !album.Tracks.Any())
                return null;

            List<PlaylistItem> items = new List<PlaylistItem>();
            foreach (string track in album.Tracks)
            {
                FileViewInfo fi = new FileViewInfo(track);
                PlaylistItem plitm = new PlaylistItem();
                PlaylistHelpers.CreateFromMediaItem(plitm, fi);
                items.Add(plitm);
            }

            return items.ToArray();
        }

        public static PlaylistItem[] GetTracks(SimpleAlbumItem album)
        {
            if (album == null || album.Tracks == null || !album.Tracks.Any())
                return null;

            List<PlaylistItem> items = new List<PlaylistItem>();
            foreach (string track in album.Tracks)
            {
                FileViewInfo fi = new FileViewInfo(track);
                PlaylistItem plitm = new PlaylistItem();
                PlaylistHelpers.CreateFromMediaItem(plitm, fi);
                items.Add(plitm);
            }

            return items.ToArray();
        }

        public static double DurationStringToSeconds(string duration)
        {
            double secs = 0;
            int idx = duration.IndexOf(':');

            if (idx <= 0)
            {
                Double.TryParse(duration, out secs);
                return secs;
            }
            int idx2;
            string s_hrs, s_mins, s_secs;
            if (duration.Count(x => x == ':') > 1)
            {

                idx2 = duration.LastIndexOf(':');
                s_hrs = duration.Substring(0, idx);
                s_mins = duration.Substring(idx + 1, idx2 - idx - 1);
                s_secs = duration.Substring(idx2 + 1);
            }
            else
            {
                s_hrs = "0";
                s_mins = duration.Substring(0, idx);
                s_secs = duration.Substring(idx + 1);
            }

            if (string.IsNullOrEmpty(s_mins))
                s_mins = "0";

            if (string.IsNullOrEmpty(s_secs))
                s_secs = "0";

            double d_hrs = Convert.ToDouble(s_hrs) * 3600;
            double d_mins = Convert.ToDouble(s_mins) * 60;
            double d_secs = Convert.ToDouble(s_secs);

            return d_hrs + d_mins + d_secs;
        }

        public static string GetDurationString(int duration)
        {
            int seconds, minutes, hours;
            hours = duration / 3600;
            minutes = (duration - (hours * 3600)) / 60;
            seconds = duration - (hours * 3600) - minutes * 60;

            string ret = "";
            if (hours > 0) ret = hours.ToString() + ":";
            if (minutes < 10) ret += "0";
            ret += minutes.ToString() + ":";
            if (seconds < 10) ret += "0";
            ret += seconds.ToString();

            return ret;
        }

        public static TimeSpan GetDuration(string durstr)
        {
            string[] tokens = durstr.Split(':');
            if (tokens.Any(to => string.IsNullOrEmpty(to.Trim())))
                return new TimeSpan();

            List<int> t = tokens.Select(str => Convert.ToInt32(str)).ToList();

            for (int i = t.Count; i < 4; i++)
                t.Insert(0, 0);

            return new TimeSpan(t[0], t[1], t[2], t[3]);
        }

        public static string SecondsToDuration(double secs)
        {
            double d_hrs = Math.Floor(secs / 3600);
            double d_mins = Math.Floor((secs - (d_hrs * 3600)) / 60);
            double d_seconds = Math.Floor(secs - (d_hrs * 3600) - (d_mins * 60));

            string dur = "";
            if (d_hrs > 0)
                dur += d_hrs.ToString() + ":";
            if (d_mins < 10)
                dur += "0";
            dur += d_mins.ToString() + ":";
            if (d_seconds < 10)
                dur += "0";
            dur += d_seconds.ToString();

            return dur;
        }

    }
}

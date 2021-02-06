using Classes;
using MpFree4k.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpFree4k.Utilies
{
    public static class LibraryUtils
    {
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

    }
}

using System;
using System.IO;
using System.Linq;

namespace Classes
{
    public static class FileViewInfoHelpers
    {
        public static void ReadMp3Fields(this FileViewInfo info)
        {
            //_mp3Fields._Handle = _Handle;
            info.Mp3Fields.FileName = info.Path;
            info.Mp3Fields.Album = (!String.IsNullOrEmpty(info._Handle.Tag.Album)) ? info._Handle.Tag.Album.Trim() : string.Empty;

            string track_artists = info._Handle.Tag.AlbumArtists.Length > 0 ? string.Join("\n", info._Handle.Tag.AlbumArtists) :
                info._Handle.Tag.Artists.Length > 0 ? string.Join("\n", info._Handle.Tag.Artists) : "unknown artist";

            info.Mp3Fields.Artists = track_artists;
            info.Mp3Fields.AlbumArtists = (info._Handle.Tag.AlbumArtists.Length > 0) ? String.Join("\n", info._Handle.Tag.AlbumArtists).Trim() : string.Empty;
            info.Mp3Fields.Comment = (!String.IsNullOrEmpty(info._Handle.Tag.Comment)) ? info._Handle.Tag.Comment.Trim() : string.Empty;
            info.Mp3Fields.Composers = (info._Handle.Tag.Composers.Length > 0) ? String.Join("\n", info._Handle.Tag.Composers).Trim() : string.Empty;
            info.Mp3Fields.Copyright = (!String.IsNullOrEmpty(info._Handle.Tag.Copyright)) ? info._Handle.Tag.Copyright.Trim() : string.Empty;
            info.Mp3Fields.Disc = info._Handle.Tag.Disc; ;
            info.Mp3Fields.DiscCount = info._Handle.Tag.DiscCount;
            info.Mp3Fields.Performers = (info._Handle.Tag.Performers.Length > 0) ? String.Join("\n", info._Handle.Tag.Performers).Trim() : string.Empty;
            info.Mp3Fields.Title = (!String.IsNullOrEmpty(info._Handle.Tag.Title)) ? info._Handle.Tag.Title.Trim() : string.Empty;
            info.Title = info.Mp3Fields.Title;
            info.Mp3Fields.Track = info._Handle.Tag.Track;
            info.Mp3Fields.TrackCount = info._Handle.Tag.TrackCount;
            info.Mp3Fields.Year = info._Handle.Tag.Year;
            info.Mp3Fields.Genres = (info._Handle.Tag.Genres.Length > 0) ? String.Join("\n", info._Handle.Tag.Genres).Trim() : string.Empty;
            info.Mp3Fields.Bitrate = info._Handle.Properties.AudioBitrate.ToString();
            info.Mp3Fields.Duration = info._Handle.Properties.Duration.ToString(@"hh\:mm\:ss");

            if (string.IsNullOrEmpty(info.Mp3Fields.AlbumArtists) && !string.IsNullOrEmpty(info.Mp3Fields.Artists))
                info.Mp3Fields.AlbumArtists = info.Mp3Fields.Artists;
        }


        public static void SetMp3FieldsToHandle(this FileViewInfo info)
        {
            if (info._Handle == null)
            {
                try
                {
                    info._Handle = TagLib.File.Create(info.Path);
                }
                catch (Exception exc)
                {
                    return;
                }
            }

            if (info._Handle == null) return;

            info._Handle.Tag.Album = info.Mp3Fields.Album.Trim();
            info._Handle.Tag.Artists = info.Mp3Fields.Artists.Trim().Split('\n');
            info._Handle.Tag.AlbumArtists = info.Mp3Fields.AlbumArtists.Trim().Split('\n');
            info._Handle.Tag.Comment = info.Mp3Fields.Comment.Trim();
            info._Handle.Tag.Composers = info.Mp3Fields.Composers.Trim().Split('\n'); ;
            info._Handle.Tag.Copyright = info.Mp3Fields.Copyright.Trim();
            info._Handle.Tag.Disc = info.Mp3Fields.Disc;
            info._Handle.Tag.DiscCount = info.Mp3Fields.DiscCount;
            info._Handle.Tag.Performers = info.Mp3Fields.Performers.Trim().Split('\n');
            info._Handle.Tag.Title = info.Mp3Fields.Title.Trim();
            info._Handle.Tag.Track = info.Mp3Fields.Track;
            info._Handle.Tag.TrackCount = info.Mp3Fields.TrackCount;
            info._Handle.Tag.Year = info.Mp3Fields.Year;
            info._Handle.Tag.Genres = info.Mp3Fields.Genres.Trim().Split('\n');
        }

        public static bool save(this FileViewInfo info, bool _explicit = false)
        {
            bool success = false;
            SetMp3FieldsToHandle(info);
            try
            {
                File.SetAttributes(info.Path, FileAttributes.Normal);
                info._Handle.Save();
                success = true;
            }
            catch (Exception exc)
            {;
            }
            ReadMp3Fields(info);
            info.Mp3Fields.HasChanged = false;
            return success;
        }

        public static void saveHandle(this FileViewInfo info)
        {
            info._Handle.Save();
            ReadMp3Fields(info);
        }
        public static bool CreateFileHandle(this FileViewInfo info)
        {
            info.FileName = System.IO.Path.GetFileName(info.Path);

            try
            {
                if (info._Handle == null)
                    info._Handle = TagLib.File.Create(info.Path);

                //ReadTagImage(info);

                info.HandleError = false;
                ReadMp3Fields(info);
                info.HasChanged = false;
                info.Mp3Fields.HasChanged = false;
                return true;
            }
            catch (Exception exc)
            {
                info.HasChanged = false;
                info.HandleError = true;
                return false;
            }
        }

        public static void ReadTagImage(this FileViewInfo info)
        {
            if (info._Handle.Tag.Pictures.Length == 0 || info._Handle.Tag.Pictures[0] == null)
            {
                info.Image = null;
                return;
            }

            byte[] raw = info._Handle.Tag.Pictures[0].Data.ToArray();


            info.Image = new System.Windows.Media.Imaging.BitmapImage();

            try
            {
                //using (var stream = new system.io.memorystream(raw))
                //{
                //    var decoder = system.windows.media.imaging.bitmapdecoder.create(stream,
                //        system.windows.media.imaging.bitmapcreateoptions.none,
                //        system.windows.media.imaging.bitmapcacheoption.onload);
                //    system.windows.media.imaging.bitmapsource src = decoder.frames[0];
                //    image = (bitmapimage)src;
                //}

                info.Image.BeginInit();
                info.Image.UriSource = null;
                info.Image.BaseUri = null;
                info.Image.StreamSource = new System.IO.MemoryStream(raw);
                info.Image.EndInit();
            }
            catch (NotSupportedException nsExc)
            {
                //info.Image = null;
                return;
            }
            catch (Exception exc)
            {
                //info.Image = null;
                System.Windows.MessageBox.Show("Exception catched: " + exc.Message);
                return;
            }
            info.Raise("Image");
        }


        public static void RemoveImage(this FileViewInfo info)
        {
            info._Handle.Tag.Pictures = null;
            save(info);

            info.Image = null;
        }


    }
}

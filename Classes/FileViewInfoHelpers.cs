﻿using Models;
using System;
using System.IO;
using System.Linq;

namespace Classes
{
    public static class FileViewInfoHelpers
    {
        public static void ReadMp3Fields(this FileViewInfo info)
        {
            info.Mp3Fields.FileName = info.Path;
            if (info._Handle == null) return;

            info.Mp3Fields.Album = (!string.IsNullOrEmpty(info._Handle.Tag.Album)) ? info._Handle.Tag.Album.Trim() : string.Empty;
            info.Mp3Fields.Artists = info._Handle.Tag.Artists.Length > 0 ? string.Join("\n", info._Handle.Tag.Artists) : "";
            info.Mp3Fields.AlbumArtists = (info._Handle.Tag.AlbumArtists.Length > 0) ? String.Join("\n", info._Handle.Tag.AlbumArtists).Trim() : info.Mp3Fields.Artists;
            info.Mp3Fields.Comment = (!String.IsNullOrEmpty(info._Handle.Tag.Comment)) ? info._Handle.Tag.Comment.Trim() : string.Empty;
            info.Mp3Fields.Composers = (info._Handle.Tag.Composers.Length > 0) ? String.Join("\n", info._Handle.Tag.Composers).Trim() : string.Empty;
            info.Mp3Fields.Copyright = (!String.IsNullOrEmpty(info._Handle.Tag.Copyright)) ? info._Handle.Tag.Copyright.Trim() : string.Empty;
            info.Mp3Fields.Disc = info._Handle.Tag.Disc; ;
            info.Mp3Fields.DiscCount = info._Handle.Tag.DiscCount;
            info.Mp3Fields.Performers = (info._Handle.Tag.Performers.Length > 0) ? String.Join("\n", info._Handle.Tag.Performers).Trim() : string.Empty;
            info.Mp3Fields.Title = (!String.IsNullOrEmpty(info._Handle.Tag.Title)) ? info._Handle.Tag.Title.Trim() : Path.GetFileNameWithoutExtension(info.Path);
            info.Title = info.Mp3Fields.Title;
            info.Mp3Fields.Track = info._Handle.Tag.Track;
            info.Mp3Fields.TrackCount = info._Handle.Tag.TrackCount;
            info.Mp3Fields.Year = info._Handle.Tag.Year;
            info.Mp3Fields.Genres = (info._Handle.Tag.Genres.Length > 0) ? String.Join("\n", info._Handle.Tag.Genres).Trim() : string.Empty;
            info.Mp3Fields.Bitrate = info._Handle.Properties.AudioBitrate.ToString();
            info.Mp3Fields.Duration = info._Handle.Properties.Duration.ToString(@"hh\:mm\:ss");
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
                    info.HandleError = true;
                    return;
                }
            }

            if (info._Handle == null)
            {
                info.HandleError = true;
                return;
            }

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

            info.HandleError = false;
            info.SetFlag(Mpfree4k.Enums.FlagType.OK);
        }

        public static bool save(this FileViewInfo info, bool _explicit = false)
        {
            bool success = false;
            SetMp3FieldsToHandle(info);

            if (info._Handle == null)
            {
                info.SetFlag(Mpfree4k.Enums.FlagType.Failures);
                return false;
            }

            try
            {
                File.SetAttributes(info.Path, FileAttributes.Normal);
                info._Handle.Save();
                ReadMp3Fields(info);
                info.Mp3Fields.HasChanged = false;
                success = true;
                info.SetFlag(Mpfree4k.Enums.FlagType.OK);
            }
            catch (Exception exc) 
            {
                info.SetFlag(Mpfree4k.Enums.FlagType.Failures);
            }
    
            return success;
        }

        public static void saveHandle(this FileViewInfo info)
        {
            info._Handle.Save();
            ReadMp3Fields(info);
        }
        public static bool CreateFileHandle(this FileViewInfo info)
        {
            info.FileName = Path.GetFileName(info.Path);

            try
            {
                if (info._Handle == null)
                    info._Handle = TagLib.File.Create(info.Path);

                info.HandleError = false;
                info.Mp3Fields.FileName = info.FileName;
                ReadMp3Fields(info);
                info.HasChanged = false;
                info.Mp3Fields.HasChanged = false;
                info.SetFlag(Mpfree4k.Enums.FlagType.OK);
                return true;
            }
            catch (Exception exc)
            {
                info.HasChanged = false;
                info.HandleError = true;
                info.SetFlag(Mpfree4k.Enums.FlagType.Failures);
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
                info.Image.BeginInit();
                info.Image.UriSource = null;
                info.Image.BaseUri = null;
                info.Image.StreamSource = new MemoryStream(raw);
                info.Image.EndInit();
            }
            catch (NotSupportedException nsExc)
            {
                return;
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show("Exception catched: " + exc.Message);
                return;
            }
            info.Raise("Image");
        }

        public static void RemoveImage(this FileViewInfo info)
        {
            if (info._Handle == null) return;
            info._Handle.Tag.Pictures = null;
            save(info);

            info.Image = null;
        }
    }
}

using System;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Drawing;
using Models;

namespace Classes
{
    public static class ImageConnector
    {
        public static void SaveImageToFile(BitmapImage img)
        {
            SaveFileDialog fDlg = new SaveFileDialog();

            fDlg.Filter = "JPEG files|.jpg";
            if (fDlg.ShowDialog() != true) return;

            string ext = Path.GetExtension(fDlg.FileName);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));

            using (var filestream = new FileStream(fDlg.FileName, System.IO.FileMode.Create))
            {
                try
                {
                    encoder.Save(filestream);
                }
                catch (Exception exc)
                {
                    System.Windows.MessageBox.Show("Exception catched: " + exc.Message);
                    return;
                }
            }

        }

        public static void ImportImage(FileViewInfo fInfo)
        {
            if (fInfo == null || fInfo._Handle == null) return;

            OpenFileDialog fDlg = new OpenFileDialog();
            fDlg.Filter = "JPG files|*.jpg";
            if (fDlg.ShowDialog() != true) return;

            string name = fDlg.FileName;
            Image img = null;

            try
            {
                img = Image.FromFile(name);
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show("Could not load image:\n" + exc.Message);
                return;
            }

            byte[] arr;
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                arr = ms.ToArray();
            }

            // Method to save album art
            TagLib.Picture pic = new TagLib.Picture();
            pic.Type = TagLib.PictureType.FrontCover;
            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
            pic.Description = "Cover";


            using (MemoryStream ms2 = new MemoryStream(arr))
            {
                img.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg);
                ms2.Position = 0;
                pic.Data = TagLib.ByteVector.FromStream(ms2);
            }

            fInfo._Handle.Tag.Pictures = new TagLib.IPicture[1] { pic };

            try
            {
                fInfo._Handle.Save();
                fInfo._Handle = null;
                fInfo.CreateFileHandle();
                FileViewInfoHelpers.ReadTagImage(fInfo);
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show(String.Format("Cannot update file '{0}'.\n{1}", fInfo.FileName, exc.Message));
            }
        }

        public static byte[] GetBytesFromBitmapImage(BitmapImage image)
        {
            if (image == null) return new byte[0];

            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(ms);

                byte[] buffer = ms.GetBuffer();
                return buffer;
            }
        }

        public static BitmapImage GetBitmapImageFromBytes(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length <= 1) return null;

            BitmapImage bi = new BitmapImage();

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                ms.Position = 0;

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();

                return image;
            }
        }

        public static byte[] getBytesFromImage(BitmapImage image)
        {
            Stream stream = image.StreamSource;
            Byte[] buffer = null;
            if (stream != null && stream.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes((Int32)stream.Length);
                }
            }

            return buffer;
        }

        public static BitmapImage getResizedImage(byte[] imageData, int decodePixelWidth, int decodePixelHeight)
        {
            BitmapImage result = new BitmapImage();
            result.BeginInit();
            result.DecodePixelWidth = decodePixelWidth;
            result.DecodePixelHeight = decodePixelHeight;
            result.StreamSource = new MemoryStream(imageData);

            result.CreateOptions = BitmapCreateOptions.None;
            result.CacheOption = BitmapCacheOption.Default;
            result.EndInit();
            return result;
        }

        public static BitmapImage GetImageFromFile(string Path)
        {
            if (!File.Exists(Path)) return null;
            try
            {
                TagLib.File FileTag = TagLib.File.Create(Path);
                if (FileTag.Tag.Pictures == null || FileTag.Tag.Pictures.Length == 0 || FileTag.Tag.Pictures[0] == null)
                    return null;

                return TagLibConvertPicture.GetImageFromTag(FileTag.Tag.Pictures);
            }
            catch (TagLib.CorruptFileException exc)
            {
                return null;
            }
        }
    }
}

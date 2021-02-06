using System;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using System.Drawing;

namespace Classes
{
    public static class ImageConnector
    {
        public static void SaveImageToFile(BitmapImage img)
        {
            SaveFileDialog fDlg = new SaveFileDialog();
            //fDlg.Filter = "Bitmap files|.bmp|EMF files|.emf|Exif files|.exif|GIF files|.gif|Icon|.ico|JPEG files|.jpg|PNG files|.png|Tiff files|.tiff";
            fDlg.Filter = "JPEG files|.jpg";
            if (fDlg.ShowDialog() != true) return;

            string ext = System.IO.Path.GetExtension(fDlg.FileName);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(img));

            using (var filestream = new System.IO.FileStream(fDlg.FileName, System.IO.FileMode.Create))
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
                img = System.Drawing.Image.FromFile(name);
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show("Could not load image:\n" + exc.Message);
                return;
            }

            byte[] arr;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                //img = System.Drawing.Image.FromStream(ms);
                arr = ms.ToArray();
            }

            // Method to save album art
            TagLib.Picture pic = new TagLib.Picture();
            pic.Type = TagLib.PictureType.FrontCover;
            pic.MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
            pic.Description = "Cover";


            using (System.IO.MemoryStream ms2 = new System.IO.MemoryStream(arr))
            {
                img.Save(ms2, System.Drawing.Imaging.ImageFormat.Jpeg); // <-- Error occurs on this line
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
                //image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.StreamSource = ms;
                image.EndInit();

                return image;
            }
        }

        public static byte[] getBytesFromImage(System.Windows.Media.Imaging.BitmapImage image)
        {
            System.IO.Stream stream = image.StreamSource;
            Byte[] buffer = null;
            if (stream != null && stream.Length > 0)
            {
                using (System.IO.BinaryReader br = new System.IO.BinaryReader(stream))
                {
                    buffer = br.ReadBytes((Int32)stream.Length);
                }
            }

            return buffer;
        }

        public static System.Windows.Media.Imaging.BitmapImage getResizedImage(byte[] imageData, int decodePixelWidth, int decodePixelHeight)
        {
            System.Windows.Media.Imaging.BitmapImage result = new System.Windows.Media.Imaging.BitmapImage();
            result.BeginInit();
            result.DecodePixelWidth = decodePixelWidth;
            result.DecodePixelHeight = decodePixelHeight;
            result.StreamSource = new System.IO.MemoryStream(imageData);

            result.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.None;
            result.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.Default;
            result.EndInit();
            return result;
        }

        public static System.Windows.Media.Imaging.BitmapImage GetImageFromFile(string Path)
        {
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

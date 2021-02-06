using MpFree4k.Enums;
using System;
using System.Windows;

namespace MpFree4k.Classes
{
    public static class SkinAdaptor
    {

        public static SkinColors ActiveSkin = SkinColors.White;

        public static void ClearStyles()
        {
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.TableView.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.TableView.ArtistView.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.TableView.AlbumView.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.TableView.TrackView.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.Playlist.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.Player.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.setctrl.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.TrackTable.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.Favourites.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.AlbumDetails.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.Playlist.PlaylistStatus.Resources.MergedDictionaries.Clear();
            MainWindow._singleton.Player.Spectrum.Resources.MergedDictionaries.Clear();

        }

        public static void AddSkin(System.Windows.ResourceDictionary resDict)
        {
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.TableView.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.TableView.ArtistView.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.TableView.AlbumView.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.TableView.TrackView.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.Playlist.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.Player.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.setctrl.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.TrackTable.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.Favourites.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.AlbumDetails.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.Playlist.PlaylistStatus.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.Player.Spectrum.Resources.MergedDictionaries.Add(resDict);
            MainWindow._singleton.Player.OnThemeChanged();
        }

        public static string GetSkinURI(SkinColors skin)
        {
            string resource_locator = string.Empty;
            switch (skin)
            {
                case SkinColors.Blue: resource_locator = @"Styles\Skins\Blue.xaml"; break;
                case SkinColors.White: resource_locator = @"Styles\Skins\White.xaml"; break;
                case SkinColors.Dark: resource_locator = @"Styles\Skins\Dark.xaml"; break;
                case SkinColors.Gray: resource_locator = @"Styles\Skins\Gray.xaml"; break;
                case SkinColors.Black_Smooth: resource_locator = @"Styles\Skins\Black_Smooth.xaml"; break;
                default: resource_locator = @"Styles\Skins\White.xaml"; break;
            }

            return resource_locator;
        }

        public static Uri GetSizeURI(FontSize size)
        {
            string resource_locator = string.Empty;
            switch (size)
            {
                case FontSize.Big: resource_locator = @"Styles\FontSizes\Big.xaml"; break;
                case FontSize.Bigger: resource_locator = @"Styles\FontSizes\Bigger.xaml"; break;
                case FontSize.Biggest: resource_locator = @"Styles\FontSizes\Biggest.xaml"; break;
                case FontSize.Huge: resource_locator = @"Styles\FontSizes\Huge.xaml"; break;
                case FontSize.Medium: resource_locator = @"Styles\FontSizes\Medium.xaml"; break;
                case FontSize.Normal: resource_locator = @"Styles\FontSizes\FontSizesNormal.xaml"; break;
                case FontSize.Small: resource_locator = @"Styles\FontSizes\Small.xaml"; break;
                case FontSize.Smaller: resource_locator = @"Styles\FontSizes\Smaller.xaml"; break;
                case FontSize.Smallest: resource_locator = @"Styles\FontSizes\Smallest.xaml"; break;
                case FontSize.Gonzo: resource_locator = @"Styles\FontSizes\Gonzo.xaml"; break;
            }

            Uri langDictUri = new Uri(resource_locator, UriKind.Relative);
            return langDictUri;
        }

        public static void AddSkin(SkinColors skin)
        {
            string uri = GetSkinURI(skin);
            if (string.IsNullOrEmpty(uri))
                return;

            Uri langDictUri = new Uri(uri, UriKind.Relative);

            if (langDictUri == null)
                return;

            System.Windows.ResourceDictionary langDict =
                System.Windows.Application.LoadComponent(langDictUri) as System.Windows.ResourceDictionary;

            AddSkin(langDict);
        }

        public static void ApplySkin(string uri)
        {
            ClearStyles();

            Uri langDictUri = new Uri(uri, UriKind.Relative);
            System.Windows.ResourceDictionary langDict =
                System.Windows.Application.LoadComponent(langDictUri) as System.Windows.ResourceDictionary;
            
            AddSkin(langDict);
        }

        public static void ApplyFontSize(MainWindow main, FontSize size)
        {
            ApplySkin(main, UserConfig.Skin, size);
            MainWindow._singleton.UpdateHeaderSize();
            MainWindow._singleton.TrackTable.UpdateMargín(size);
            MainWindow._singleton.Favourites.UpdateMargín(size);
        }

        public static void ApplySkin(MainWindow main, SkinColors skin, FontSize size)
        {
            ResourceDictionary orig_dict = Application.Current.Resources;

            ClearStyles();

            AddSkin(skin);

            string colors = @"Styles\SolidColorBrushes.xaml";
            Uri colors_uri = new Uri(colors, UriKind.Relative);
            System.Windows.ResourceDictionary brushes_dict =
               System.Windows.Application.LoadComponent(colors_uri) as System.Windows.ResourceDictionary;

            AddSkin(brushes_dict);

            Uri size_uri = GetSizeURI(size);
            System.Windows.ResourceDictionary sizes_dict =
               System.Windows.Application.LoadComponent(size_uri) as System.Windows.ResourceDictionary;

            AddSkin(sizes_dict);

            ActiveSkin = skin;

            string res = Application.Current.Resources.MergedDictionaries[0].Values.ToString();
            foreach (var key in sizes_dict.Keys)
            {
                //Application.Current.Resources[key.ToString()] = sizes_dict.Values.
            }



        }

        public static void ApplySkinFromFile(MainWindow main, string skinFile)
        {
        }

        public static void ApplySkinByName(MainWindow main, string skinName, string fileName = "")
        {
            FontSize current_size = UserConfig.FontSize;
            switch (skinName)
            {
                case "White": ApplySkin(main, SkinColors.White, current_size); break;
                case "Blue": ApplySkin(main, SkinColors.Blue, current_size); break;
                case "Dark": ApplySkin(main, SkinColors.Dark, current_size); break;
                case "Custom": ApplySkinFromFile(main, fileName); break;
                case "URI": ApplySkin(fileName); break;
                default: ApplySkin(main, SkinColors.White, current_size); break;
            }
        }
    }
}

using MpFree4k;
using Mpfree4k.Enums;
using System;
using System.Windows;
using Configuration;

namespace Classes
{
    public static class SkinAdaptor
    {

        public static SkinColors ActiveSkin = SkinColors.White;

        public static void ClearStyles()
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.TableView.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.TableView.ArtistView.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.TableView.AlbumView.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.TableView.TrackView.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.Playlist.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.Player.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.SmartPlayer.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.setctrl.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.TrackTable.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.Favourites.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.AlbumDetails.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.Playlist.PlaylistStatus.Resources.MergedDictionaries.Clear();
            //MainWindow.Instance.Player.Spectrum.Resources.MergedDictionaries.Clear();
            MainWindow.Instance.SmartPlayer.Spectrum.Resources.MergedDictionaries.Clear();

        }

        public static void AddSkin(ResourceDictionary resDict)
        {
            Application.Current.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.TableView.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.TableView.ArtistView.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.TableView.AlbumView.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.TableView.TrackView.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.Playlist.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.Player.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.SmartPlayer.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.setctrl.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.TrackTable.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.Favourites.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.AlbumDetails.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.Playlist.PlaylistStatus.Resources.MergedDictionaries.Add(resDict);
            //MainWindow.Instance.Player.Spectrum.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.Player.OnThemeChanged();
            MainWindow.Instance.SmartPlayer.Spectrum.Resources.MergedDictionaries.Add(resDict);
            MainWindow.Instance.SmartPlayer.OnThemeChanged();
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

            ResourceDictionary langDict = Application.LoadComponent(langDictUri) as ResourceDictionary;

            AddSkin(langDict);
        }

        public static void ApplySkin(string uri)
        {
            ClearStyles();

            Uri langDictUri = new Uri(uri, UriKind.Relative);
            ResourceDictionary langDict = Application.LoadComponent(langDictUri) as ResourceDictionary;
            
            AddSkin(langDict);
        }

        public static void ApplyFontSize(MainWindow main, FontSize size)
        {
            ApplySkin(main, UserConfig.Skin, size);
            MainWindow.Instance.TrackTable.UpdateMargín(size);
            MainWindow.Instance.Favourites.UpdateMargín(size);
        }

        public static void ApplySkin(MainWindow main, SkinColors skin, FontSize size)
        {
            ResourceDictionary orig_dict = Application.Current.Resources;

            ClearStyles();
            AddSkin(skin);

            string colors = @"Styles\SolidColorBrushes.xaml";
            Uri colors_uri = new Uri(colors, UriKind.Relative);
            ResourceDictionary brushes_dict = Application.LoadComponent(colors_uri) as ResourceDictionary;

            AddSkin(brushes_dict);

            Uri size_uri = GetSizeURI(size);
            ResourceDictionary sizes_dict = Application.LoadComponent(size_uri) as ResourceDictionary;

            AddSkin(sizes_dict);

            ActiveSkin = skin;

            //Application.Current.Resources.MergedDictionaries[0].Values.ToString();
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

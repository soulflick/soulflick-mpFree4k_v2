using MpFree4k;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Configuration;
using Mpfree4k.Enums;
using Classes;
using ViewModels;

namespace Dialogs
{
    public partial class SettingControl : Window, INotifyPropertyChanged
    {
        static string settingsFile = "Settings.xml";

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        private bool clicked = false;
        public bool HasConfig = false;

        public SettingControl()
        {
            DataContext = this;

            Loaded += SettingControl_Loaded;

            InitializeComponent();

            readConfig();

            autoSavePlaylist.IsChecked = UserConfig.AutoSavePlaylist;
            showAlbumArtists.IsChecked = UserConfig.ShowFullAlbum;
            rememberSelected.IsChecked = UserConfig.RememberSelectedAlbums;
            showPathInPlaylist.IsChecked = UserConfig.ShowPathInPlaylist;
            showPathInLibrary.IsChecked = UserConfig.ShowPathInLibrary;
        }

        private void SettingControl_Loaded(object sender, RoutedEventArgs e)
        {
            sldFontSize.Value = (int)UserConfig.FontSize;
            comboSkins.SelectedIndex = (int)UserConfig.Skin;
            comboPluginType.SelectedIndex = (int)UserConfig.PluginType;
            sldPadding.Value = (int)UserConfig.PaddingType;

            Array sizes = Enum.GetValues(typeof(ControlSize));

            int s = -1;
            foreach (int size in sizes)
            {
                s++;
                if (size == (int)UserConfig.ControlSize)
                {
                    sldControlSizes.Value = s;
                    break;
                }
            }

            comboSkins.SelectionChanged += ComboBoxSkin_SelectionChanged;
            sldFontSize.ValueChanged += SldFontSize_ValueChanged;
            comboPluginType.SelectionChanged += comboPluginType_SelectionChanged;
        }

        private void SldFontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            FontSize size = (FontSize)sldFontSize.Value;
            UserConfig.FontSize = size;
            
            if (MainWindow.Instance.TableView.ArtistView != null)
                SkinAdaptor.ApplyFontSize(MainWindow.Instance, size);
            
            SkinAdaptor.ApplyPadding(MainWindow.Instance, UserConfig.PaddingType);

            MainWindow.Instance.Player.Raise("ButtonSize");
            MainWindow.Instance.SmartPlayer.Raise("ButtonSize");
        }

        private void ComboSizes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!clicked)
                return;

            ComboBoxItem ci = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (ci == null)
                return;

            FontSize size = (FontSize)ci.Tag;
            UserConfig.FontSize = size;
            if (MainWindow.Instance.TableView.ArtistView != null)
                SkinAdaptor.ApplyFontSize(MainWindow.Instance, size);
            SkinAdaptor.ApplyPadding(MainWindow.Instance, UserConfig.PaddingType);
            MainWindow.Instance.Player.Raise("ButtonSize");
            MainWindow.Instance.SmartPlayer.Raise("ButtonSize");

        }

        void readConfig()
        {
            if (!File.Exists(settingsFile))
                return;

            HasConfig = true;
            XDocument doc = XDocument.Load(settingsFile);

            var keys = doc.Descendants("setting");
            foreach (var key in keys)
            {
                if (key.Attribute("name") == null ||
                   key.Attribute("value") == null)
                    continue;

                string key_str = key.Attribute("name").Value.ToLower();
                string key_val = key.Attribute("value").Value.ToLower();

                if (key_str.ToLower() == "skin")
                {
                    if (key_val == "gray") UserConfig.Skin = SkinColors.Gray;
                    else if (key_val == "dark") UserConfig.Skin = SkinColors.Dark;
                    else if (key_val == "blue") UserConfig.Skin = SkinColors.Blue;
                    else if (key_val == "black_smooth") UserConfig.Skin = SkinColors.Black_Smooth;
                    else UserConfig.Skin = SkinColors.White;
                }

                else if (key_str.ToLower() == "fontsize")
                {
                    int a = 0;
                    if (int.TryParse(key_val, out a))
                    {
                        if (Enum.IsDefined(typeof(FontSize), a))
                        {
                            UserConfig.FontSize = (FontSize)a;
                        }
                    }
                }

                else if (key_str.ToLower() == "controlsize")
                {
                    int a = 0;
                    if (int.TryParse(key_val, out a))
                    {
                        if (Enum.IsDefined(typeof(ControlSize), a))
                        {
                            UserConfig.ControlSize = (ControlSize)a;
                        }
                    }
                }

                else if (key_str == "controlpadding")
                {
                    int a = 0;
                    if (int.TryParse(key_val, out a))
                    {
                        if (Enum.IsDefined(typeof(PaddingType), a))
                        {
                            UserConfig.PaddingType = (PaddingType)a;
                        }
                    }
                }

                else if (key_str == "artistviewtype")
                {
                    int a = 0;
                    if (int.TryParse(key_val, out a))
                    {
                        if (Enum.IsDefined(typeof(ArtistViewType), a))
                        {
                            UserConfig.ArtistViewType = (ArtistViewType)a;
                        }
                    }
                }

                else if (key_str == "albumviewtype")
                {
                    int a = 0;
                    if (int.TryParse(key_val, out a))
                    {
                        if (Enum.IsDefined(typeof(AlbumViewType), a))
                        {
                            UserConfig.AlbumViewType = (AlbumViewType)a;
                        }
                    }
                }

                else if (key_str == "trackviewtype")
                {
                    int a = 0;
                    if (int.TryParse(key_val, out a))
                    {
                        if (Enum.IsDefined(typeof(TrackViewType), a))
                        {
                            UserConfig.TrackViewType = (TrackViewType)a;
                        }
                    }
                }

                else if (key_str == "autosaveplaylist")
                {
                    bool _true = true;
                    if (bool.TryParse(key_val, out _true))
                        UserConfig.AutoSavePlaylist = _true;
                }

                else if (key_str == "showfullalbum")
                {
                    bool _true = false;
                    if (bool.TryParse(key_val, out _true))
                        UserConfig.ShowFullAlbum = _true;
                }

                else if (key_str == "rememberalbums")
                {
                    bool _true = false;
                    if (bool.TryParse(key_val, out _true))
                        UserConfig.RememberSelectedAlbums = _true;
                }

                else if (key_str == "showpathinplaylist")
                {
                    bool _true = false;
                    if (bool.TryParse(key_val, out _true))
                        UserConfig.ShowPathInPlaylist = _true;
                }

                else if (key_str == "showpathinlibrary")
                {
                    bool _true = false;
                    if (bool.TryParse(key_val, out _true))
                        UserConfig.ShowPathInLibrary = _true;
                }

                else if (key_str == "numbertracks")
                {
                    int a = 0;
                    if (int.TryParse(key_val, out a))
                    {
                        UserConfig.NumberRecentTracks = a;
                        tbNumTracks.Text = a.ToString();
                    }
                }

                else if (key_str == "numberalbums")
                {
                    int a = 0;
                    if (int.TryParse(key_val, out a))
                    {
                        UserConfig.NumberRecentAlbums = a;
                        tbNumAlbums.Text = a.ToString();
                    }
                }

                else if (key_str == "plugin")
                {
                    int a = 0;
                    if (int.TryParse(key_val, out a))
                    {
                        if (Enum.IsDefined(typeof(PluginTypes), a))
                        {
                            UserConfig.PluginType = (PluginTypes)a;
                        }
                    }
                }
            }
        }

        string sanitize(string str)
        {
            char[] _c = new char[] { '/', '\\', '"', '\t', '\n', '\r' };
            foreach (char c in _c)
                str = str.Replace(c, '_');
            return str;
        }

        private void ComboBoxSkin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!clicked)
                return;

            ComboBoxItem ci = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (ci == null)
                return;

            SkinColors selected_Color = (SkinColors)ci.Tag;

            if (MainWindow.Instance.TableView.ArtistView != null)
                SkinAdaptor.ApplySkin(MainWindow.Instance, selected_Color, UserConfig.FontSize);

            UserConfig.Skin = selected_Color;
        }

        private void _This_Closing(object sender, CancelEventArgs e)
        {
            UserConfig.ShowFullAlbum = showAlbumArtists.IsChecked == true;
            UserConfig.AutoSavePlaylist = autoSavePlaylist.IsChecked == true;
            UserConfig.RememberSelectedAlbums = rememberSelected.IsChecked == true;
            UserConfig.ShowPathInPlaylist = showPathInPlaylist.IsChecked == true;
            UserConfig.ShowPathInLibrary = showPathInLibrary.IsChecked == true;

            WriteUserConfig();

            PlaylistViewModel.Instance.ShowPathInPlaylist = UserConfig.ShowPathInPlaylist;
            TrackTableViewModel.Instance.ShowPathInLibrary = UserConfig.ShowPathInLibrary;
        }

        public static void WriteUserConfig()
        {
            string doc = "<xml>\n\t<settings>\n";
            doc += "\t\t<setting name=\"plugin\" value=\"" + (int)UserConfig.PluginType + "\"/>\n";
            doc += "\t\t<setting name=\"skin\" value=\"" + UserConfig.Skin.ToString() + "\"/>\n";
            doc += "\t\t<setting name=\"fontsize\" value=\"" + (int)UserConfig.FontSize + "\"/>\n";
            doc += "\t\t<setting name=\"controlsize\" value=\"" + (int)UserConfig.ControlSize + "\"/>\n";
            doc += "\t\t<setting name=\"controlpadding\" value=\"" + (int)UserConfig.PaddingType + "\"/>\n";
            doc += "\t\t<setting name=\"artistviewtype\" value=\"" + (int)UserConfig.ArtistViewType + "\"/>\n";
            doc += "\t\t<setting name=\"albumviewtype\" value=\"" + (int)UserConfig.AlbumViewType + "\"/>\n";
            doc += "\t\t<setting name=\"trackviewtype\" value=\"" + (int)UserConfig.TrackViewType + "\"/>\n";
            doc += "\t\t<setting name=\"autosaveplaylist\" value=\"" + (bool)UserConfig.AutoSavePlaylist + "\"/>\n";
            doc += "\t\t<setting name=\"showfullalbum\" value=\"" + (bool)UserConfig.ShowFullAlbum + "\"/>\n";
            doc += "\t\t<setting name=\"rememberalbums\" value=\"" + (bool)UserConfig.RememberSelectedAlbums + "\"/>\n";
            doc += "\t\t<setting name=\"showpathinplaylist\" value=\"" + (bool)UserConfig.ShowPathInPlaylist + "\"/>\n";
            doc += "\t\t<setting name=\"showpathinlibrary\" value=\"" + (bool)UserConfig.ShowPathInLibrary + "\"/>\n";
            doc += "\t\t<setting name=\"numbertracks\" value=\"" + UserConfig.NumberRecentTracks + "\"/>\n";
            doc += "\t\t<setting name=\"numberalbums\" value=\"" + UserConfig.NumberRecentAlbums + "\"/>\n";
            doc += "\t</settings>\n</xml>";

            File.WriteAllText(settingsFile, doc);
        }

        private void comboBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) => clicked = true;

        private void comboControlSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem itm = (sender as ComboBox).SelectedItem as ComboBoxItem;
            ControlSize csize = (ControlSize)itm.Tag;
            UserConfig.ControlSize = csize;
            MainWindow.Instance.Player.Raise("ButtonSize");
            MainWindow.Instance.SmartPlayer.Raise("ButtonSize");
        }

        private void tbNumAlbums_TextChanged(object sender, TextChangedEventArgs e)
        {
            int d = 0;
            if (int.TryParse((sender as TextBox).Text, out d))
            {
                if (d > 0 && d <= 640)
                    UserConfig.NumberRecentAlbums = d;
            }
        }

        private void tbNumTracks_TextChanged(object sender, TextChangedEventArgs e)
        {
            int d = 0;
            if (int.TryParse((sender as TextBox).Text, out d))
            {
                if (d > 0 && d <= 16000)
                    UserConfig.NumberRecentTracks = d;
            }
        }

        private void comboPluginType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem itm = (sender as ComboBox).SelectedItem as ComboBoxItem;
            PluginTypes pType = (PluginTypes)itm.Tag;
            UserConfig.PluginType = pType;

            WriteUserConfig();

            if (MessageBox.Show("Do you want to apply your settings now?", "Playback Plugin", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                PlayerViewModel.Instance.Rebuild();
            }
        }

        private void sldPadding_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int amount = (int)sldPadding.Value;
            UserConfig.PaddingType = (PaddingType)amount;

            ComboBoxItem ci =  comboSkins?.SelectedItem as ComboBoxItem;
            if (ci == null)
                return;

            SkinColors selected_Color = (SkinColors)ci.Tag;

            SkinAdaptor.ApplySkin(MainWindow.Instance, selected_Color, UserConfig.FontSize);
            SkinAdaptor.ApplyPadding(MainWindow.Instance, UserConfig.PaddingType);
        }

        private void sldControlSizes_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int index = (int)sldControlSizes.Value;
            int control_size = 16;

            Array sizes = Enum.GetValues(typeof(ControlSize));
            int s = -1;

            foreach (int size in sizes)
            {
                s++;
                if (s == index)
                {
                    control_size = size;
                    break;
                }
            }

            ControlSize csize = (ControlSize)control_size;
            UserConfig.ControlSize = csize;
            MainWindow.Instance.Player.Raise("ButtonSize");
            MainWindow.Instance.SmartPlayer.Raise("ButtonSize");
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) => Close();
    }
}

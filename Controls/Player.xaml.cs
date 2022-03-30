using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Timers;
using Classes;
using ViewModels;
using System.Windows.Threading;
using Interfaces;
using Equalizer;
using Models;
using Mpfree4k.Enums;
using Configuration;
using MpFree4k;
using System.Windows.Media;
using MpFree4k.Classes;

namespace Controls
{
    public class ValueChangedEvent : EventArgs
    {
        public string Key;
        public string Value;
    }

    public class ViewModelChangedEventArgs : EventArgs
    {
        public ViewModelChangedEventArgs(object info) { this.value = info; }
        public object value;
    }

    
    public partial class Player : UserControl, INotifyPropertyChanged
    {
        public static Player Instance;
        public event PropertyChangedEventHandler PropertyChanged;
        private PlaylistViewModel _playlistVM = null;
        
        private System.Timers.Timer CheckSongTimer;
        public event EventHandler<ValueChangedEvent> ValueChanged;
        public event EventHandler<ViewModelChangedEventArgs> ViewModelChanged;

        public Player()
        {
            InitializeComponent();

            Instance = this;

            CheckSongTimer = new System.Timers.Timer(300);
            CheckSongTimer.AutoReset = true;
        }

        public static ImageSource defaultImage = null;

        public double ButtonSize => (double)UserConfig.ControlSize;

        public bool SongEnded => PlayerViewModel.Instance.MediaPlayer.SongEnded;

        public void updateProgress(string label, double progress)
        {
            lblProgress.Content = label;
            UpdatePositionMarker(progress);
        }



        private bool _isCheckedState = false;
        public bool IsCheckedState
        {
            get => _isCheckedState;
            set { _isCheckedState = value; Raise(nameof(IsCheckedState)); }
        }

        private Visibility _touchButtonsVisibility = Visibility.Visible;
        public Visibility TouchButtonsVisibility
        {
            get => _touchButtonsVisibility;
            set
            {
                _touchButtonsVisibility = value;
                Raise(nameof(TouchButtonsVisibility));
            }
        }

        public void Raise(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        void UpdatePositionMarker(double position)
        {
            sldTrackSlider.ValueChanged -= sldTrackSlider_ValueChanged;
            sldTrackSlider.Value = position;
            sldTrackSlider.ValueChanged += sldTrackSlider_ValueChanged;
        }

        void InitPositionMarker(int duration) => 0.ToString(); //=> sldTrackSlider.Maximum = duration;

        void setProgressSliderTicks(double duration)
        {
            sldTrackSlider.TickFrequency = 10; // (int)(duration / 30);
            System.Windows.Media.DoubleCollection dbls = new System.Windows.Media.DoubleCollection();
            for (int i = 0; i <= (int)(duration / 30); i++)
                dbls.Add(i * 30);
            sldTrackSlider.Ticks = dbls;
        }

        public Dispatcher dispatcher => this.Dispatcher;

        public void sldTrackSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PlayerViewModel.Instance.MediaPlayer == null) return;
            if (!PlayerViewModel.Instance.MediaPlayer.HasMedia) return;
            //PlayerViewModel.Instance.MediaPlayer.Position = sldTrackSlider.Value;
        }

        private void lblProgress_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            PlayerViewModel.Instance.remainingMode = PlayerViewModel.Instance.remainingMode == RemainingMode.Elapsed ? RemainingMode.Remaining : RemainingMode.Elapsed;

    }
}

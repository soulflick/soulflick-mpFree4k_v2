using Microsoft.Win32;
using MpFree4k.Classes;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using WPFEqualizer;

namespace MpFree4k.ViewModels
{
    public class EqualizerWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseProperty(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ObservableCollection<LabelLocation> Presets { get; set; } = new ObservableCollection<LabelLocation>();

        public string EqualizerDirectory => Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Presets");

        public EqualizerWindowViewModel()
        {
            Reload();
        }

        private int? _selectedIndex = 0;
        public int? SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value < 0)
                    _selectedIndex = null;
                else
                    _selectedIndex = value;

                RaiseProperty(nameof(SelectedIndex));
            }
        }

        public void Save()
        {
            if (SpectrumViewModel.Instance._equalizer == null)
                return;

            var myPath = EqualizerDirectory;

            if (!Directory.Exists(myPath))
                Directory.CreateDirectory(myPath);

            var dlg = new SaveFileDialog
            {
                Title = "Save EQ Settings to .json File",
                InitialDirectory = myPath,
                DefaultExt = "csv",
                Filter = "Any ASCII file (*.csv)|*.csv|All files (*.*)|*.*",
                AddExtension = true,
                RestoreDirectory = true
            };

            if (dlg.ShowDialog() != true)
                return;

            var newPath = dlg.FileName;

            var settings = SpectrumViewModel.Instance._equalizer.SampleFilters.Select(filter => filter.AverageGainDB.ToString());
            string values = string.Join(";", settings);
            File.WriteAllText(newPath, values);

            Reload(newPath);
        }

        public double[] GetEQFrom(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException(file);

            var values = new double[SpectrumViewModel.EqualizerBandCount];
            var content = File.ReadAllText(file);
            var numbers = content.Split(';').Select(c => c.Trim());
            int index = -1;

            foreach (var number in numbers)
            {
                index++;
                if (double.TryParse(number, out double value))
                {
                    values[index] = value;
                }
            }
            return values;
        }

        private void Reload(string preset_file = null)
        {
            var directory = EqualizerDirectory;
            if (!Directory.Exists(directory))
                return;

            Presets = null;
            SelectedIndex = null;

            Presets = new ObservableCollection<LabelLocation>();

            try
            {
                var files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    Presets.Add(new LabelLocation { Label = name, Path = file });
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show($"Cannot load presets from {directory}!.\n{exc.Message}");
                return;
            }

            RaiseProperty(nameof(Presets));

            if (string.IsNullOrEmpty(preset_file))
            {
                SelectedIndex = 0;
                return;
            }

            var item = Presets.FirstOrDefault(preset => preset.Path.Equals(preset_file));
            if (item == null) return;

            SelectedIndex = Presets.IndexOf(item);
        }
    }
}

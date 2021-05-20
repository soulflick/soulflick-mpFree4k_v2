using Models;
using System.Windows.Controls;

namespace Controls
{
    public partial class AudioPropertiesView : UserControl
    {
        public AudioPropertiesView() => InitializeComponent();

        public void ClearInfo()
        {
            tbBitrate.Text = string.Empty;
            tbBitsPerSample.Text = string.Empty;
            tbChannels.Text = string.Empty;
            tbDescription.Text = string.Empty;
            tbDuration.Text = string.Empty;
            tbSampleRate.Text = string.Empty;
            tbCodecs.Text = string.Empty;
        }

        public void SetInfo(FileViewInfo info)
        {
            tbBitrate.Text = info._Handle.Properties.AudioBitrate.ToString();
            tbBitsPerSample.Text = info._Handle.Properties.BitsPerSample.ToString();
            tbChannels.Text = info._Handle.Properties.AudioChannels.ToString();
            tbDescription.Text = info._Handle.Properties.Description;
            tbDuration.Text = info._Handle.Properties.Duration.ToString(@"hh\:mm\:ss");
            tbSampleRate.Text = info._Handle.Properties.AudioSampleRate.ToString();

            tbCodecs.Text = string.Empty;
            foreach (var entry in info._Handle.Properties.Codecs)
            {
                if (entry == null) continue;
                tbCodecs.Text += entry.MediaTypes.ToString() + " ";
            }
        }
    }
}

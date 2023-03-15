using Avalonia.Controls;

namespace SoundFlux.Views
{
    public partial class ServerView : StackPanel
    {
        public ServerView()
        {
            InitializeComponent();

            channelsComboBox.Items = new int[] { 0, 1, 2 };
            sampleRateComboBox.Items = new int[] { 0, 8000, 16000, 24000, 32000, 44100, 48000 };
            bitDepthComboBox.Items = new int[] { 0, 16, 32 };
        }
    }
}

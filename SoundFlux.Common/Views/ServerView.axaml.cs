using Avalonia.Controls;

namespace SoundFlux.Views
{
    public partial class ServerView : StackPanel
    {
        public ServerView()
        {
            InitializeComponent();

            channelsComboBox.ItemsSource = new int[] { 0, 1, 2 };
            sampleRateComboBox.ItemsSource = new int[] { 0, 8000, 16000, 24000, 32000, 44100, 48000 };
            //bitDepthComboBox.ItemsSource = new int[] { 0, 16, 32 };
            bitDepthComboBox.ItemsSource = new int[] { 0, 16 };
        }
    }
}

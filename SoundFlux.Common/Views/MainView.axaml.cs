using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Threading;

namespace SoundFlux.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();

            MainTabControl.ItemsPanel = new FuncTemplate<IPanel>(() => new Grid()
            {
                ColumnDefinitions = new ColumnDefinitions("Auto * *")
            });

            GlobalContext.OnExitEvent += () =>
                SharedSettings.Instance.AddSection("MainView").Add("SelectedPage", MainTabControl.SelectedIndex);

            GlobalContext.OnErrorEvent += msg =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    DefaultPopupText.Text = msg;
                    DefaultPopup.Open();
                });
            };

            var sect = SharedSettings.Instance.GetSection("MainView");
            if (sect == null) return;
            MainTabControl.SelectedIndex = sect.GetInt("SelectedPage");
        }
    }
}

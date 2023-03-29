using CommunityToolkit.Mvvm.ComponentModel;
using SoundFlux.Services;
using SoundFlux.Views;

namespace SoundFlux.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private int selectedTabIndex = 0;

        public ClientViewModel ClientVM { get; private set; }
        public ServerViewModel ServerVM { get; private set; }
        public SettingsViewModel SettingsVM { get; private set; }
        public TrayIconViewModel? TrayIconVM { get; private set; }

        // supply main window on desktop platform to have a tray icon
        public MainViewModel(Client client, Server server, MainWindow? mainWindow = null)
        {
            ClientVM = new(client);
            ServerVM = new(server);

            if (mainWindow != null)
                TrayIconVM = new(mainWindow);

            SettingsVM = new(TrayIconVM);

            SelectedTabIndex = ServiceRegistry.SettingsManager.Get("MainViewModel", "SelectedTabIndex", 0);
        }

        public void SaveSettings()
        {
            ServiceRegistry.SettingsManager.Set("MainViewModel", "SelectedTabIndex", SelectedTabIndex);
            ClientVM.SaveSettings();
            ServerVM.SaveSettings();
            SettingsVM.SaveSettings();
            TrayIconVM?.SaveSettings();
        }
    }
}

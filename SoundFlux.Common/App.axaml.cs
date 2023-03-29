using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SoundFlux.Services;
using SoundFlux.ViewModels;
using SoundFlux.Views;

namespace SoundFlux
{
    public partial class App : Application
    {
        private MainViewModel? mainVM;
        private MainWindow? mainWindow;
        private Client client;
        private Server server;

        public App(Client client, Server server)
        {
            this.client = client;
            this.server = server;
        }

        public override void Initialize()
            => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // init window
                mainWindow = new();

                // init main VM
                mainVM = new(client, server, mainWindow);

                mainWindow.Content = new MainView()
                {
                    DataContext = mainVM
                };
                mainWindow.DataContext = mainVM?.TrayIconVM;
                DataContext = mainVM?.TrayIconVM;

                desktop.MainWindow = mainWindow;
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                mainVM = new(client, server);
                singleViewPlatform.MainView = new MainView()
                {
                    DataContext = mainVM
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void SaveSettings()
            => mainVM?.SaveSettings();
    }
}

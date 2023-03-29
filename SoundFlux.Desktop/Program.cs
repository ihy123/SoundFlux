using Avalonia;
using SoundFlux.Desktop.Services;
using SoundFlux.Services;
using System;

namespace SoundFlux.Desktop
{
    internal class Program
    {
        private static Client client = new();
        private static Server server = new();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            if (!OperatingSystem.IsWindows())
                throw new Exception($"This OS is not supported.");

            ServiceRegistry.ErrorHandler = new MessageBoxErrorHandler();
            ServiceRegistry.SettingsManager = new SettingsManagerWin32();
            ServiceRegistry.NetHelper = new NetHelper();

            try
            {
                ServiceRegistry.SettingsManager.Load();
                client.LoadSettings();
                server.LoadSettings();
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                ServiceRegistry.ErrorHandler.Error(e.ToString());
            }

            try
            {
                client.SaveSettings();
                server.SaveSettings();

                App? app = (App?)Application.Current;
                app?.SaveSettings();

                ServiceRegistry.SettingsManager.Save();
            }
            catch (Exception e)
            {
                ServiceRegistry.ErrorHandler.Error(e.ToString());
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure(() => new App(client, server)).UsePlatformDetect().LogToTrace();
    }
}

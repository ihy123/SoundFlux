using Avalonia;
using SoundFlux.Desktop.Services;
using SoundFlux.Services;
using System;

namespace SoundFlux.Desktop
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            if (!OperatingSystem.IsWindows())
                throw new Exception($"This OS is not supported.");

            ServiceRegistry.SettingsManager = new SettingsManagerWin32();
            ServiceRegistry.NetHelper = new NetHelper();
            ServiceRegistry.ErrorHandler = new MessageBoxErrorHandler();

            try
            {
                ServiceRegistry.SettingsManager.Load();
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                ServiceRegistry.ErrorHandler.Error(e.ToString());
            }

            try
            {
                ServiceRegistry.SettingsManager.Save();
            }
            catch (Exception e)
            {
                ServiceRegistry.ErrorHandler.Error(e.ToString());
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
    }
}

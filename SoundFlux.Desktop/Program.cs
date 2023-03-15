using Avalonia;
using Avalonia.Platform;
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
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            GlobalContext.OnExit();
            SharedSettings.Instance.Save();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            var builder = AppBuilder
                .Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();

            var os = builder.RuntimePlatform.GetRuntimeInfo().OperatingSystem;
            if (os == OperatingSystemType.WinNT)
                new PlatformUtilsWin32();
            else
                throw new Exception($"This platform is not supported ({os})");

            SharedSettings.Instance.Load();

            return builder;
        }
    }
}

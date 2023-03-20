using Avalonia;
using Avalonia.Platform;
using System;
using System.Runtime.InteropServices;

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
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
                GlobalContext.OnExit();
                SharedSettings.Instance.Save();
            }
            catch (Exception e)
            {
                ErrorMessageBox(IntPtr.Zero, e.ToString(), "Exception");
            }
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

        [DllImport("User32", EntryPoint = "MessageBoxW", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private extern static bool ErrorMessageBox(IntPtr hWnd, string description, string caption, uint type = 0x10);
    }
}

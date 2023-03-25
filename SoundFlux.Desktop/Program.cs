using Avalonia;
using System;
using System.Runtime.InteropServices;

namespace SoundFlux.Desktop
{
    internal partial class Program
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
                GlobalEvents.OnExit();
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
            if (OperatingSystem.IsWindows())
                new PlatformUtilsWin32();
            else
                throw new Exception($"This OS is not supported.");

            SharedSettings.Instance.Load();
            return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
        }

        [LibraryImport("User32", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ErrorMessageBox(IntPtr hWnd, string description, string caption, uint type = 0x10);
    }
}

using SoundFlux.Services;
using System;
using System.Runtime.InteropServices;

namespace SoundFlux.Desktop.Services
{
    internal partial class MessageBoxErrorHandler : IErrorHandler
    {
        public void Error(string message)
            => ErrorMessageBox(IntPtr.Zero, message, "Exception");

        [LibraryImport("User32", EntryPoint = "MessageBoxW", StringMarshalling = StringMarshalling.Utf16)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ErrorMessageBox(IntPtr hWnd, string description, string caption, uint type = 0x10);
    }
}

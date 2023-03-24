using SoundFlux.Audio.Device;
using System.Collections.Generic;

namespace SoundFlux
{
    public static class Utils
    {
        public static bool ValidateIpv4WithPort(string? addr)
        {
            int colon = addr != null ? addr.IndexOf(':') : -1;
            if (colon == -1 || TryParsePort(addr!.Substring(colon + 1)) == -1)
                return false;

            string[] octets = addr.Substring(0, colon).Split('.');
            if (octets.Length != 4)
                return false;

            foreach (string o in octets)
                if (!byte.TryParse(o, out _))
                    return false;

            return true;
        }

        // returns -1 if port is invalid
        public static int TryParsePort(string? port)
        {
            if (string.IsNullOrEmpty(port) || !int.TryParse(port, out int r) || r < 0 || r > 65535)
                return -1;
            return r;
        }

        public static int HandleFromDeviceIndex(IEnumerable<IAudioDevice>? devices, int index, int defaultValue = 0)
        {
            if (devices != null && index >= 0)
                foreach (var d in devices)
                    if (index-- == 0)
                        return d.Handle;
            return defaultValue;
        }

        public static int DeviceIndexFromHandle(IEnumerable<IAudioDevice>? devices, int handle, int defaultValue = 0)
        {
            if (devices != null)
            {
                int i = 0;
                foreach (var d in devices)
                {
                    if (handle == d.Handle)
                        return i;
                    ++i;
                }
            }
            return defaultValue;
        }
    }
}

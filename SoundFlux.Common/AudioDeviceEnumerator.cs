using ManagedBass;
using System.Collections.Generic;

namespace SoundFlux
{
    public static class AudioDeviceEnumerator
    {
        #region Input

        public const int DefaultInputDeviceIndex = 0;

        // input device infos mapped by device index
        public static Dictionary<int, DeviceInfo> InputDevices
        {
            get
            {
                // enumerate BASS input devices
                var infos = new Dictionary<int, DeviceInfo>();
                for (int i = 0; ; ++i)
                {
                    if (!Bass.RecordGetDeviceInfo(i, out DeviceInfo info))
                        break;
                    if (info.IsEnabled)
                        infos.Add(i, info);
                }
                return infos;
            }
        }

        #endregion

        #region Output

        public const int DefaultOutputDeviceIndex = 1;

        // output device infos mapped by device index
        public static Dictionary<int, DeviceInfo> OutputDevices
        {
            get
            {
                // enumerate BASS output devices
                var infos = new Dictionary<int, DeviceInfo>();
                for (int i = 0; ; ++i)
                {
                    if (!Bass.GetDeviceInfo(i, out DeviceInfo info))
                        break;
                    if (info.IsEnabled)
                        infos.Add(i, info);
                }
                return infos;
            }
        }

        #endregion
    }
}

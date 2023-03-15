using ManagedBass;
using System.Collections.Generic;

namespace SoundFlux.Audio.Device
{
    public class OutputDevice : IAudioDevice
    {
        public string? Name { get; private set; }
        public bool IsSystemDefault { get; private set; }
        public bool IsDefault => Name == "Default";
        public int BassIndex { get; private set; }

        private static OutputDevice? noSoundDevice = null;
        public static OutputDevice NoSound
        {
            get
            {
                if (noSoundDevice == null)
                {
                    Bass.GetDeviceInfo(0, out DeviceInfo info);
                    noSoundDevice = new OutputDevice(info, 0);
                    noSoundDevice.Initialize();
                }
                return noSoundDevice;
            }
        }

        public static List<OutputDevice> List
        {
            get
            {
                // enumerate BASS output devices
                var outputDevices = new List<OutputDevice>();
                for (int i = 0; ; ++i)
                {
                    if (!TryCreate(i, out OutputDevice? device))
                        break;
                    if (device != null)
                        outputDevices.Add(device);
                }
                return outputDevices;
            }
        }

        private bool initialized = false;

        /// <summary>
        /// Try to instantiate from BASS device index
        /// </summary>
        /// <param name="bassIndex">BASS device index</param>
        /// <param name="device">Returns null when device is not enabled or it is a "No sound" device</param>
        /// <returns>True when index is valid, false when index is out of range</returns>
        private static bool TryCreate(int bassIndex, out OutputDevice? device)
        {
            device = null;
            if (Bass.GetDeviceInfo(bassIndex, out DeviceInfo info))
            {
                if (info.IsEnabled) // && bassIndex != 0
                    device = new OutputDevice(info, bassIndex);
                return true;
            }
            return false;
        }

        public void Initialize()
        {
            if (!initialized)
            {
                if (!Bass.Init(BassIndex))
                    throw new BassException();
                initialized = true;
            }
        }

        public void Shutdown()
        {
            if (initialized)
            {
                // ignore possible exception from Select()
                try
                {
                    Select();
                    Bass.Free();
                }
                catch (BassException) { }
                initialized = false;
            }
        }

        public void Select()
            => Bass.CurrentDevice = BassIndex;

        private OutputDevice(DeviceInfo info, int bassIndex)
        {
            BassIndex = bassIndex;
            Name = info.Name;
            IsSystemDefault = info.IsDefault;
        }

        ~OutputDevice() => Shutdown();
    }
}

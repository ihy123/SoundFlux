using ManagedBass;
using System.Collections.Generic;

namespace SoundFlux.Audio.Device
{
    public class OutputDevice : IAudioDevice
    {
        public const int DefaultHandle = 1;
        public int Handle { get; private set; }
        public string? Name { get; private set; }
        public bool IsSystemDefault { get; private set; }
        public bool IsDefault => Name == "Default";

        private bool initialized = false;

        private static OutputDevice? noSoundDevice = null;
        public static OutputDevice NoSound
        {
            get
            {
                if (noSoundDevice == null)
                {
                    if (!TryCreate(0, out noSoundDevice))
                        throw new BassException();
                    noSoundDevice!.Initialize();
                }
                return noSoundDevice;
            }
        }

        // List enabled Output devices
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

        public void Initialize()
        {
            if (!initialized)
            {
                if (!Bass.Init(Handle))
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
            => Bass.CurrentDevice = Handle;

        private OutputDevice(DeviceInfo info, int bassIndex)
        {
            Handle = bassIndex;
            Name = info.Name;
            IsSystemDefault = info.IsDefault;
        }

        ~OutputDevice() => Shutdown();

        private static bool TryCreate(int bassIndex, out OutputDevice? device)
        {
            device = null;
            if (Bass.GetDeviceInfo(bassIndex, out DeviceInfo info))
            {
                if (info.IsEnabled)
                    device = new OutputDevice(info, bassIndex);
                return true;
            }
            return false;
        }
    }
}

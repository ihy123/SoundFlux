using ManagedBass;
using System.Collections.Generic;

namespace SoundFlux.Audio.Device
{
    public class InputDevice : IAudioDevice
    {
        public string? Name { get; private set; }
        public bool IsSystemDefault { get; private set; }
        public bool IsDefault => Name == "Default";
        public int Channels { get; private set; } = 0;
        public int SampleRate { get; private set; } = 0;
        public bool IsLoopback { get; private set; }
        public int BassIndex { get; private set; }

        public static List<InputDevice> List
        {
            get
            {
                // enumerate BASS record devices
                var inputDevices = new List<InputDevice>();
                for (int i = 0; ; ++i)
                {
                    if (!TryCreate(i, out InputDevice? device))
                        break;
                    if (device != null)
                        inputDevices.Add(device);
                }
                return inputDevices;
            }
        }

        private bool initialized = false;

        /// <summary>
        /// Try to instantiate from BASS record device index
        /// </summary>
        /// <param name="bassIndex">BASS record device index</param>
        /// <param name="device">Returns null when device is not enabled or it is a "No sound" device</param>
        /// <returns>True when index is valid, false when index is out of range</returns>
        private static bool TryCreate(int bassIndex, out InputDevice? device)
        {
            device = null;
            if (Bass.RecordGetDeviceInfo(bassIndex, out DeviceInfo info))
            {
                if (info.IsEnabled) // && bassIndex != 0
                    device = new InputDevice(info, bassIndex);
                return true;
            }
            return false;
        }

        public void Initialize()
        {
            if (!initialized)
            {
                if (!Bass.RecordInit(BassIndex) ||
                    !Bass.RecordGetInfo(out RecordInfo bassRecordInfo))
                    throw new BassException();
                Channels = bassRecordInfo.Channels;
                SampleRate = bassRecordInfo.Frequency;
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
                    Bass.RecordFree();
                }
                catch (BassException) { }
                initialized = false;
            }
        }

        public void Select()
            => Bass.CurrentRecordingDevice = BassIndex;

        private InputDevice(DeviceInfo info, int bassIndex)
        {
            BassIndex = bassIndex;
            Name = info.Name;
            IsSystemDefault = info.IsDefault;
            IsLoopback = info.IsLoopback;
        }

        ~InputDevice() => Shutdown();
    }
}

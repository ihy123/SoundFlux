using ManagedBass;
using System.Collections.Generic;

namespace SoundFlux.Audio.Device
{
    public class InputDevice : IAudioDevice
    {
        public const int DefaultHandle = 0;
        public int Handle { get; private set; }
        public string? Name { get; private set; }
        public bool IsSystemDefault { get; private set; }
        public bool IsDefault => Name == "Default";
        public int Channels { get; private set; } = 0;
        public int SampleRate { get; private set; } = 0;
        public bool IsLoopback { get; private set; }

        private bool initialized = false;

        // List enabled Input devices
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

        public void Initialize()
        {
            if (!initialized)
            {
                if (!Bass.RecordInit(Handle) ||
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
            => Bass.CurrentRecordingDevice = Handle;

        private InputDevice(DeviceInfo info, int bassIndex)
        {
            Handle = bassIndex;
            Name = info.Name;
            IsSystemDefault = info.IsDefault;
            IsLoopback = info.IsLoopback;
        }

        ~InputDevice() => Shutdown();

        private static bool TryCreate(int bassIndex, out InputDevice? device)
        {
            device = null;
            if (Bass.RecordGetDeviceInfo(bassIndex, out DeviceInfo info))
            {
                if (info.IsEnabled)
                    device = new InputDevice(info, bassIndex);
                return true;
            }
            return false;
        }
    }
}

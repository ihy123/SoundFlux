using Avalonia.Platform;
using System;
using System.Collections.Generic;

namespace SoundFlux
{
    public abstract class PlatformUtilities
    {
        private static PlatformUtilities? instance;
        public static PlatformUtilities Instance
        {
            get
            {
                if (instance == null)
                    throw new Exception("PlatformUtilities can't be used before initialization");
                return instance;
            }
            protected set => instance = value;
        }

        public abstract OperatingSystemType OS { get; }

        public abstract string SettingsDirectory { get; }

        public abstract string GetDeviceName();

        public abstract List<string> GetNetworkInterfaceAddressList();
    }
}

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
                    throw new Exception("PlatformUtilities.Instance is not initialized");
                return instance;
            }
            protected set => instance = value;
        }

        public abstract string SettingsDirectory { get; }

        public abstract string DeviceName { get; }

        public abstract List<string> NetworkInterfaceAddressList { get; }
    }
}

using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Linq;
using System;

namespace SoundFlux.Desktop
{
    public partial class PlatformUtilsWin32 : PlatformUtilities
    {
        public override string SettingsDirectory => Environment.ExpandEnvironmentVariables("%AppData%\\SoundFlux\\");

        public PlatformUtilsWin32() => Instance = this;

        public override string DeviceName
        {
            get
            {
                string name;
                try
                {
                    name = Environment.MachineName;
                }
                catch
                {
                    name = Resources.Resources.Unnamed;
                }
                return name;
            }
        }

        public override List<string> NetworkInterfaceAddressList
        {
            get
            {
                var list = new List<string>();
                foreach (NetworkInterface i in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipprops = i.GetIPProperties();
                    if (i.OperationalStatus == OperationalStatus.Up &&
                        i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var ipv4 = ipprops.UnicastAddresses?
                            .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                            .Address;

                        if (ipv4 != null && IsHardwareNetworkInterface((uint)ipprops.GetIPv4Properties().Index))
                            list.Add(ipv4.ToString());
                    }
                }
                return list;
            }
        }

        [LibraryImport("NetworkInterfaceHelper")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool IsHardwareNetworkInterface(uint interfaceIndex);
    }
}

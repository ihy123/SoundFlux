using SoundFlux.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace SoundFlux.Desktop.Services
{
    internal partial class NetHelper : INetHelper
    {
        public string DeviceName
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

        public List<string> NetworkInterfaceAddressList
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

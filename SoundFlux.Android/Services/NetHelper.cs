using Android.App;
using Android.OS;
using Android.Provider;
using SoundFlux.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace SoundFlux.Android.Services
{
    internal class NetHelper : INetHelper
    {
        private static readonly BuildVersionCodes SdkVer = Build.VERSION.SdkInt;

        public string DeviceName
        {
            get
            {
                string? name = null;
                if (SdkVer >= BuildVersionCodes.NMr1)
                    name = Settings.Global.GetString(Application.Context.ContentResolver, Settings.Global.DeviceName);
                if (name == null && SdkVer <= BuildVersionCodes.S)
                    name = Settings.Secure.GetString(Application.Context.ContentResolver, "bluetooth_name");
                if (name != null)
                    return name!;
                if (Build.Model != null)
                    return Build.Model;
                else
                    return Resources.Resources.Unnamed;
            }
        }

        public List<string> NetworkInterfaceAddressList
        {
            get
            {
                List<string> list = new(), list1 = new();

                foreach (NetworkInterface i in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (i.OperationalStatus == OperationalStatus.Up &&
                        i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var ipv4 = i.GetIPProperties().UnicastAddresses?
                            .FirstOrDefault(a => a.Address.AddressFamily ==
                                System.Net.Sockets.AddressFamily.InterNetwork)?
                            .Address;
                        if (ipv4 == null)
                            continue;

                        if ((i.Name.Contains("ap") || i.Name.Contains("wlan")) &&
                            !i.Name.Contains("dummy"))
                        {
                            list.Add(ipv4.ToString());
                        }
                        else
                            list1.Add(ipv4.ToString());
                    }
                }

                if (list.Count == 0)
                    return list1;

                return list;
            }
        }
    }
}

using Android.OS;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Android.App;
using Android.Provider;

namespace SoundFlux.Android
{
    internal class PlatformUtilsAndroid : PlatformUtilities
    {
        private static readonly BuildVersionCodes SdkVer = Build.VERSION.SdkInt;
        private string configPath;

        public PlatformUtilsAndroid(string configPath)
        {
            Instance = this;
            this.configPath = configPath;
        }

        public override string SettingsDirectory => configPath;

        public override string DeviceName
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

        public override List<string> NetworkInterfaceAddressList
        {
            get
            {
                var list = new List<string>();
                foreach (NetworkInterface i in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (i.OperationalStatus == OperationalStatus.Up &&
                        i.Name.Contains("wlan") &&
                        i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var ipv4 = i.GetIPProperties().UnicastAddresses?
                            .FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                            .Address;

                        if (ipv4 != null)
                            list.Add(ipv4.ToString());
                    }
                }
                return list;
            }
        }
    }
}

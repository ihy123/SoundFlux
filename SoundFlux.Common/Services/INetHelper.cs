using System.Collections.Generic;

namespace SoundFlux.Services
{
    public interface INetHelper
    {
        string DeviceName { get; }

        List<string> NetworkInterfaceAddressList { get; }
    }
}

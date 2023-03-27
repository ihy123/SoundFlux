using System.Collections.Generic;

namespace SoundFlux.Services.Dummy
{
    internal class DummyNetHelper : INetHelper
    {
        public string DeviceName => string.Empty;

        public List<string> NetworkInterfaceAddressList => new();
    }
}

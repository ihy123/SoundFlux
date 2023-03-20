using ManagedBass;
using SoundFlux.Audio.Device;
using System;
using System.Text;

namespace SoundFlux.Audio.Stream
{
    internal class NetworkClientStream : Stream
    {
        public IAudioDevice Device { get; private set; }

        private string addrAndHeaders;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverAddress">Server address string in form of IP:Port</param>
        public NetworkClientStream(IAudioDevice device, string serverAddress,
            string[]? httpHeaders = null, DownloadProcedure? callback = null, Action? onDisconnected = null)
        {
            Device = device;

            StringBuilder sb = new StringBuilder("http://");
            sb.Append(serverAddress);
            sb.Append("\r\n");
            if (httpHeaders != null)
            {
                foreach (var header in httpHeaders)
                {
                    sb.Append(header);
                    sb.Append("\r\n");
                }
            }
            addrAndHeaders = sb.ToString();

            Device.Select();

            Handle = Bass.CreateStream(addrAndHeaders, 0, BassFlags.StreamDownloadBlocks, callback);
            if (Handle == 0)
                throw new BassException();

            if (0 == Bass.ChannelSetSync(Handle, SyncFlags.Downloaded | SyncFlags.Mixtime,
                0, (h, c, d, u) => onDisconnected?.Invoke()))
                throw new BassException();
        }
    }
}

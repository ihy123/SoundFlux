using ManagedBass;
using SoundFlux.Audio.Device;
using SoundFlux.Audio.Stream;
using System;

namespace SoundFlux
{
    public class Client
    {
        public int ConnectTimeOut
        {
            get => Bass.NetTimeOut;
            set => Bass.NetTimeOut = value;
        }

        public double Volume
        {
            get => volume;
            set
            {
                volume = value;
                if (stream != null)
                    stream.Volume = volume;
            }
        }

        private OutputDevice? device;
        private NetworkClientStream? stream;
        private double volume = 1.0;

        public Client()
        {
            Bass.NetPreBuffer = 0;
            Bass.DeviceNonStop = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="clientName"></param>
        /// <param name="networkBufferDurationMs"></param>
        /// <param name="playbackBufferDurationMs"></param>
        /// <param name="disconnectedCallback">Invoked when disconnected by an external reason
        /// (server terminated, connection lost etc), but not by Stop()</param>
        /// <param name="reconnectedCallback"></param>
        /// <returns></returns>
        public bool Start(OutputDevice device, string serverAddress, string clientName,
            int networkBufferDurationMs = 5000, int playbackBufferDurationMs = 500,
            Action? disconnectedCallback = null)
        {
            this.device = device;

            device.Initialize();
            Bass.NetBufferLength = networkBufferDurationMs;
            Bass.PlaybackBufferLength = playbackBufferDurationMs;

            stream = new NetworkClientStream(device, serverAddress, new string[] { "SFName:" + clientName }, null, disconnectedCallback);
            stream.Volume = volume;

            return stream.Play();
        }

        public void Stop()
        {
            device?.Shutdown();
            device = null;
            stream?.Stop();
            stream = null;
        }
    }
}

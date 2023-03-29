using ManagedBass;
using System;
using System.Text;

namespace SoundFlux.Services
{
    public class Client
    {
        private int streamHandle = 0;

        public int CurrentOutputDeviceIndex { get; private set; } = -1;

        public int NextOutputDeviceIndex { get; set; } = AudioDeviceEnumerator.DefaultOutputDeviceIndex;

        // in ms; affects only clients started after
        public static int ConnectTimeOut
        {
            get => Bass.NetTimeOut;
            set => Bass.NetTimeOut = value;
        }
        public const double DefaultConnectTimeOut = 5000;

        // in ms; affects only clients started after
        public static int NetworkBufferDuration
        {
            get => Bass.NetBufferLength;
            set => Bass.NetBufferLength = value;
        }
        public const double DefaultNetworkBufferDuration = 5000;

        // in ms; affects only clients started after
        public static int PlaybackBufferDuration
        {
            get => Bass.PlaybackBufferLength;
            set => Bass.PlaybackBufferLength = value;
        }
        public const double DefaultPlaybackBufferDuration = 500;

        // values from 0.0 to 1.0 (but can go above 1.0, it will amplify)
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = value;

                if (streamHandle == 0) return;

                if(!Bass.ChannelSetAttribute(streamHandle,
                    ChannelAttribute.Volume, isMuted ? 0.0 : _volume))
                    throw new BassException();
            }
        }
        private double _volume = 1.0;

        public bool IsMuted
        {
            get => isMuted;
            set
            {
                if (isMuted == value)
                    return;

                isMuted = value;
                Volume = _volume;
            }
        }
        private bool isMuted = false;

        public Client()
        {
            Bass.NetPreBuffer = 0;
            Bass.DeviceNonStop = true;
        }

        // disconnectedCallback: Invoked when disconnected by an external reason
        // (server terminated, connection lost etc), but not by Stop()
        public virtual bool Start(string serverAddress, string clientName,
            Action? disconnectedCallback = null)
        {
            if (!Bass.Init(NextOutputDeviceIndex))
            {
                if (Bass.LastError != Errors.Already)
                    throw new BassException();

                // if device is already initialized, set it
                Bass.CurrentDevice = NextOutputDeviceIndex;
            }
            CurrentOutputDeviceIndex = NextOutputDeviceIndex;

            // construct url
            StringBuilder url = new("http://");
            url.Append(serverAddress).Append("\r\n");
            url.Append("SFName:").Append(clientName).Append("\r\n");

            // create network stream
            streamHandle = Bass.CreateStream(url.ToString(), 0, BassFlags.StreamDownloadBlocks, null);
            if (streamHandle == 0)
                throw new BassException();

            // set disconnected callback
            if (disconnectedCallback != null && 0 == Bass.ChannelSetSync(
                streamHandle, SyncFlags.Downloaded | SyncFlags.Mixtime,
                0, (h, c, d, u) => disconnectedCallback.Invoke()))
                throw new BassException();

            // set current volume
            Volume = _volume;

            // play stream
            return Bass.ChannelPlay(streamHandle, false);
        }

        public virtual void Stop()
        {
            // ignore possible exception while selecting device
            try
            {
                if (CurrentOutputDeviceIndex != -1)
                {
                    Bass.CurrentDevice = CurrentOutputDeviceIndex;
                    Bass.Free();
                }
            }
            catch (BassException) { }

            Bass.ChannelStop(streamHandle);
            CurrentOutputDeviceIndex = -1;
            streamHandle = 0;
        }

        public virtual void SaveSettings()
        {
            var sm = ServiceRegistry.SettingsManager;
            sm.Set("Client", "NextOutputDeviceIndex",
                CurrentOutputDeviceIndex != -1 ? CurrentOutputDeviceIndex : NextOutputDeviceIndex);
            sm.Set("Client", "IsMuted", IsMuted);
            sm.Set("Client", "Volume", Volume);
            sm.Set("Client", "ConnectTimeOut", ConnectTimeOut);
            sm.Set("Client", "NetworkBufferDuration", NetworkBufferDuration);
            sm.Set("Client", "PlaybackBufferDuration", PlaybackBufferDuration);
        }

        public virtual void LoadSettings()
        {
            var sm = ServiceRegistry.SettingsManager;
            NextOutputDeviceIndex = sm.Get("Client", "NextOutputDeviceIndex", NextOutputDeviceIndex);
            IsMuted = sm.Get("Client", "IsMuted", IsMuted);
            Volume = sm.Get("Client", "Volume", Volume);
            ConnectTimeOut = sm.Get("Client", "ConnectTimeOut", ConnectTimeOut);
            NetworkBufferDuration = sm.Get("Client", "NetworkBufferDuration", NetworkBufferDuration);
            PlaybackBufferDuration = sm.Get("Client", "PlaybackBufferDuration", PlaybackBufferDuration);
        }
    }
}

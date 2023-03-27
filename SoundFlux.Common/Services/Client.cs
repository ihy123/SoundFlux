﻿using ManagedBass;
using System;
using System.Text;

namespace SoundFlux.Services
{
    public class Client
    {
        private int deviceIndex = AudioDeviceEnumerator.DefaultOutputDeviceIndex;
        private int streamHandle = 0;

        public static int ConnectTimeOut
        {
            get => Bass.NetTimeOut;
            set => Bass.NetTimeOut = value;
        }

        public double Volume
        {
            get => streamHandle == 0 ? 1.0 :
                Bass.ChannelGetAttribute(streamHandle, ChannelAttribute.Volume);
            set
            {
                if (streamHandle == 0)
                    throw new InvalidOperationException("Client is not started");

                if (!Bass.ChannelSetAttribute(streamHandle, ChannelAttribute.Volume, value))
                    throw new BassException();
            }
        }

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
        public virtual bool Start(int outputDeviceIndex, string serverAddress, string clientName,
            int networkBufferDurationMs = 5000, int playbackBufferDurationMs = 500,
            Action? disconnectedCallback = null)
        {
            deviceIndex = outputDeviceIndex;

            if (!Bass.Init(deviceIndex))
            {
                if (Bass.LastError != Errors.Already)
                    throw new BassException();

                // if device is already initialized, set it
                Bass.CurrentDevice = deviceIndex;
            }

            Bass.NetBufferLength = networkBufferDurationMs;
            Bass.PlaybackBufferLength = playbackBufferDurationMs;

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

            // play stream
            return Bass.ChannelPlay(streamHandle, false);
        }

        public virtual void Stop()
        {
            // ignore possible exception while selecting device
            try
            {
                Bass.CurrentDevice = deviceIndex;
                Bass.Free();
            }
            catch (BassException) { }

            Bass.ChannelStop(streamHandle);
            deviceIndex = streamHandle = 0;
        }
    }
}
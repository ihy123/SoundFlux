using ManagedBass;
using ManagedBass.Enc;
using System;
using System.Text;

namespace SoundFlux.Services
{
    public class Server
    {
        public delegate bool ClientCallback(bool isConnecting,
            string clientAddress, string? clientName);

        private int streamHandle = 0;
        private ClientCallback? callback;
        private EncodeClientProcedure? encodeClientProc;

        public int CurrentInputDeviceIndex { get; private set; } = -1;

        public int NextInputDeviceIndex { get; set; } = AudioDeviceEnumerator.DefaultInputDeviceIndex;

        // in ms; affects only servers started after
        public double ServerBufferDuration { get; set; } = DefaultServerBufferDuration;
        public const double DefaultServerBufferDuration = 2000;

        // in ms; affects only servers started after
        public double RecordingPollingPeriod { get; set; } = DefaultRecordingPollingPeriod;
        public const double DefaultRecordingPollingPeriod = 30;

        // affects only servers started after
        public string NextPort { get; set; } = "0";

        public int CurrentPort { get; private set; } = 0;

        // affects only servers started after
        public int TransmissionChannels { get; set; } = 0;

        // affects only servers started after
        public int TransmissionSampleRate { get; set; } = 0;

        // affects only servers started after
        public bool TransmissionFloatSamples { get; set; } = true;

        public Server()
            => Bass.Configure(Configuration.LoopbackRecording, true);

        public virtual bool Start(ClientCallback? clientCallback = null)
        {
            callback = clientCallback;
            encodeClientProc = (callback == null) ? null : EncodeClientProc;

            if (!Bass.RecordInit(NextInputDeviceIndex))
            {
                if (Bass.LastError != Errors.Already)
                    throw new BassException();

                // if device is already initialized, set it
                Bass.CurrentRecordingDevice = NextInputDeviceIndex;
            }
            CurrentInputDeviceIndex = NextInputDeviceIndex;

            // validate parameters
            if (Bass.RecordGetInfo(out RecordInfo bassRecordInfo))
            {
                if (bassRecordInfo.Channels == 0 && TransmissionChannels == 0)
                    TransmissionChannels = 1;
                if (bassRecordInfo.Frequency == 0 && TransmissionSampleRate == 0)
                    TransmissionSampleRate = 44100;
            }

            RecordProcedure recordCallback = (a, b, c, d) => true;

            // try initialize with selected bit depth
            streamHandle = Bass.RecordStart(TransmissionSampleRate, TransmissionChannels,
                BassFlags.RecordPause | (TransmissionFloatSamples ? BassFlags.Float : 0),
                (int)RecordingPollingPeriod, recordCallback);

            if (streamHandle == 0 && Bass.LastError == Errors.SampleFormat)
            {
                // try initialize with other bit depth
                streamHandle = Bass.RecordStart(TransmissionSampleRate, TransmissionChannels,
                    BassFlags.RecordPause | (TransmissionFloatSamples ? 0 : BassFlags.Float),
                    (int)RecordingPollingPeriod, recordCallback);
            }

            if (streamHandle == 0)
                throw new BassException();

            // select no sound device
            if (!Bass.Init(0) && Bass.LastError != Errors.Already)
                throw new BassException();

            Bass.CurrentDevice = 0;

            // get server buffer size in bytes
            int bufSize = (int)Bass.ChannelSeconds2Bytes(streamHandle, ServerBufferDuration / 1000.0);
            if (bufSize == -1)
                throw new BassException();

            // create dummy stream
            GetSamplesInfo(out int channels, out int sampleRate, out bool floatSamples);
            int dummyStream = Bass.CreateStream(sampleRate, channels, BassFlags.Decode |
                (floatSamples ? BassFlags.Float : 0), StreamProcedureType.Dummy);

            if (dummyStream == 0)
                throw new BassException();

            // set up encoder on the dummy stream
            int encodeHandle = BassEnc.EncodeStart(dummyStream, null,
                EncodeFlags.PCM | EncodeFlags.AutoFree, (a, b, c, d, e) => { });
            if (encodeHandle == 0)
                throw new BassException();

            // start server
            CurrentPort = BassEnc.ServerInit(encodeHandle, NextPort, bufSize, 512, 0, encodeClientProc, 0);
            if (CurrentPort == 0)
                throw new BassException();

            // submit silence to the dummy stream
            byte[] silence = new byte[512];
            Bass.ChannelGetData(dummyStream, silence, silence.Length);

            // move encoder to original stream
            if (!BassEnc.EncodeSetChannel(encodeHandle, streamHandle))
                throw new BassException();

            // stop dummy encoder
            Bass.ChannelStop(dummyStream);

            return Bass.ChannelPlay(streamHandle, false);
        }

        public virtual void Stop()
        {
            // ignore possible exception while selecting device
            try
            {
                if (CurrentInputDeviceIndex != -1)
                {
                    Bass.CurrentRecordingDevice = CurrentInputDeviceIndex;
                    Bass.RecordFree();
                }
            }
            catch (BassException) { }

            Bass.ChannelStop(streamHandle);
            CurrentInputDeviceIndex = -1;
            CurrentPort = streamHandle = 0;
            callback = null;
            encodeClientProc = null;
        }

        public void GetSamplesInfo(out int channels, out int sampleRate, out bool floatSamples)
        {
            if (streamHandle == 0)
                throw new InvalidOperationException("Server is not initialized");

            if (!Bass.ChannelGetInfo(streamHandle, out ChannelInfo info))
                throw new BassException();

            channels = info.Channels;
            sampleRate = info.Frequency;
            floatSamples = info.Resolution == Resolution.Float;
        }

        public virtual void SaveSettings()
        {
            var sm = ServiceRegistry.SettingsManager;
            sm.Set("Server", "NextInputDeviceIndex",
                CurrentInputDeviceIndex != -1 ? CurrentInputDeviceIndex : NextInputDeviceIndex);
            sm.Set("Server", "ServerBufferDuration", ServerBufferDuration);
            sm.Set("Server", "RecordingPollingPeriod", RecordingPollingPeriod);
            sm.Set("Server", "NextPort", CurrentPort == 0 ? NextPort : CurrentPort.ToString());
            sm.Set("Server", "TransmissionChannels", TransmissionChannels);
            sm.Set("Server", "TransmissionSampleRate", TransmissionSampleRate);
            sm.Set("Server", "TransmissionFloatSamples", TransmissionFloatSamples);
        }

        public virtual void LoadSettings()
        {
            var sm = ServiceRegistry.SettingsManager;
            NextInputDeviceIndex = sm.Get("Server", "NextInputDeviceIndex", NextInputDeviceIndex);
            ServerBufferDuration = sm.Get("Server", "ServerBufferDuration", ServerBufferDuration);
            RecordingPollingPeriod = sm.Get("Server", "RecordingPollingPeriod", RecordingPollingPeriod);
            NextPort = sm.Get("Server", "NextPort", NextPort)!;
            TransmissionChannels = sm.Get("Server", "TransmissionChannels", TransmissionChannels);
            TransmissionSampleRate = sm.Get("Server", "TransmissionSampleRate", TransmissionSampleRate);
            TransmissionFloatSamples = sm.Get("Server", "TransmissionFloatSamples", TransmissionFloatSamples);
        }

        private bool EncodeClientProc(int handle,
            bool isConnecting, string address, IntPtr headers, IntPtr user)
        {
            if (headers == 0)
                return callback!(isConnecting, address, null);

            string? name = null;

            unsafe
            {
                bool isNull = false, prevIsNull = false;
                byte* ptr = (byte*)headers;
                for (int i = 0; ; ++i)
                {
                    isNull = ptr[i] == 0;
                    if (isNull)
                    {
                        if (prevIsNull)
                            break;

                        // process current header
                        var parts = Encoding.ASCII.GetString(ptr, i).Split(':');
                        if (parts[0].Contains("sfname", StringComparison.OrdinalIgnoreCase))
                        {
                            name = parts[1].Trim(' ');
                            break;
                        }

                        ptr += i + 1;
                        i = 0;
                    }
                    prevIsNull = isNull;
                }
            }

            return callback!(isConnecting, address, name);
        }
    }
}

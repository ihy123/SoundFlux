using ManagedBass;
using ManagedBass.Enc;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoundFlux
{
    public class Server
    {
        public delegate bool ClientCallback(bool isConnecting, string clientAddress, string? clientName);

        private int deviceIndex = 0, streamHandle = 0;
        private ClientCallback? callback;
        private EncodeClientProcedure? encodeClientProc;

        public const int DefaultInputDeviceIndex = 0;

        // input device infos mapped by device index
        public static Dictionary<int, DeviceInfo> InputDevices
        {
            get
            {
                // enumerate BASS input devices
                var infos = new Dictionary<int, DeviceInfo>();
                for (int i = 0; ; ++i)
                {
                    if (!Bass.RecordGetDeviceInfo(i, out DeviceInfo info))
                        break;
                    if (info.IsEnabled)
                        infos.Add(i, info);
                }
                return infos;
            }
        }

        public int CurrentPort { get; private set; }

        public Server()
            => Bass.Configure(Configuration.LoopbackRecording, true);

        public bool Start(int inputDeviceIndex, int serverBufferDurationMs, string port,
            int recordingPollingPeriod, ClientCallback? clientCallback = null,
            int transmissionChannels = 0, int transmissionSampleRate = 0,
            bool transmissionFloatSamples = true)
        {
            deviceIndex = inputDeviceIndex;
            callback = clientCallback;

            if (!Bass.RecordInit(deviceIndex))
            {
                if (Bass.LastError != Errors.Already)
                    throw new BassException();

                // if device is already initialized, set it
                Bass.CurrentRecordingDevice = deviceIndex;
            }

            if (!Bass.RecordGetInfo(out RecordInfo bassRecordInfo))
                throw new BassException();

            // validate parameters
            if (bassRecordInfo.Channels == 0 && transmissionChannels == 0)
                transmissionChannels = 1;
            if (bassRecordInfo.Frequency == 0 && transmissionSampleRate == 0)
                transmissionSampleRate = 44100;

            RecordProcedure recordCallback = (a, b, c, d) => true;

            // try initialize with selected bit depth
            streamHandle = Bass.RecordStart(transmissionSampleRate, transmissionChannels,
                BassFlags.RecordPause | (transmissionFloatSamples ? BassFlags.Float : 0),
                recordingPollingPeriod, recordCallback);

            if (streamHandle == 0 && Bass.LastError == Errors.SampleFormat)
            {
                // try initialize with other bit depth
                streamHandle = Bass.RecordStart(transmissionSampleRate, transmissionChannels,
                    BassFlags.RecordPause | (transmissionFloatSamples ? 0 : BassFlags.Float),
                    recordingPollingPeriod, recordCallback);
            }

            if (streamHandle == 0)
                throw new BassException();

            // select no sound device
            if (!Bass.Init(0) && Bass.LastError != Errors.Already)
                throw new BassException();

            Bass.CurrentDevice = 0;

            // get server buffer size in bytes
            int bufSize = (int)Bass.ChannelSeconds2Bytes(streamHandle, serverBufferDurationMs / 1000.0);
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
            InitEncodeClientProc();
            CurrentPort = BassEnc.ServerInit(encodeHandle, port, bufSize, 512, 0, encodeClientProc, 0);
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

        public void Stop()
        {
            // ignore possible exception while selecting device
            try
            {
                Bass.CurrentRecordingDevice = deviceIndex;
                Bass.RecordFree();
            }
            catch (BassException) { }

            Bass.ChannelStop(streamHandle);
            deviceIndex = streamHandle = 0;
            callback = null;
            encodeClientProc = null;
        }

        private void InitEncodeClientProc()
        {
            if (callback == null)
                encodeClientProc = null;
            else
            {
                encodeClientProc = (a, connecting, addr, hdrs, u) =>
                {
                    if (hdrs == 0)
                        return callback(connecting, addr, null);

                    string? name = null;

                    unsafe
                    {
                        bool isNull = false, prevIsNull = false;
                        byte* ptr = (byte*)hdrs;
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

                    return callback(connecting, addr, name);
                };
            }
        }
    }
}

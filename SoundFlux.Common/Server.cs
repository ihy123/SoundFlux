using ManagedBass;
using SoundFlux.Audio.Device;
using SoundFlux.Audio.DSP;
using SoundFlux.Audio.Stream;

namespace SoundFlux
{
    public class Server
    {
        public delegate bool ClientCallback(bool isConnecting, string clientAddress, string? clientName);

        public InputDevice? Device { get; private set; }

        public RecordStream? Stream { get; private set; }

        public int Port => server == null ? 0 : server.Port;

        public int RecordingPollingPeriod
        {
            get => recordingPollingPeriod;
            set
            {
                if (IsStarted)
                    throw new System.Exception("Attempted to set RecordingPollingPeriod while server is running");

                recordingPollingPeriod = value;
            }
        }

        public bool IsStarted { get; private set; }

        private int recordingPollingPeriod;

        private Encoder? encoder;
        private EncoderServer? server;
        private ClientCallback? callback;

        public Server()
        {
            if (!Bass.GetConfigBool(Configuration.LoopbackRecording))
                Bass.Configure(Configuration.LoopbackRecording, true);
        }

        public bool Start(InputDevice device, int serverBufferDurationMs, int port,
            ClientCallback? clientCallback = null,
            int transmissionChannels = 0, int transmissionSampleRate = 0,
            bool transmissionFloatSamples = true)
        {
            if (IsStarted)
                return false;

            Device = device;
            device.Initialize();

            if (device.Channels == 0 && transmissionChannels == 0)
                transmissionChannels = 1;
            if (device.SampleRate == 0 && transmissionSampleRate == 0)
                transmissionSampleRate = 44100;

            Stream = new RecordStream(device, transmissionChannels,
                transmissionSampleRate, transmissionFloatSamples, recordingPollingPeriod);

            callback = clientCallback;

            var dummy = new DummyStream(OutputDevice.NoSound, Stream.SampleRate, Stream.Channels, Stream.FloatSamples);
            encoder = new PcmEncoder(dummy);

            server = new EncoderServer(encoder, port.ToString(),
                serverBufferDurationMs, 512, callback == null ? null : (isConnecting, clientAddress, httpHeaders) =>
                {
                    string? name = null;

                    if (httpHeaders != null)
                    {
                        foreach (string h in httpHeaders)
                        {
                            var parts = h.Split(':');
                            if (parts[0].Contains("sfname", System.StringComparison.OrdinalIgnoreCase))
                            {
                                name = parts[1].Trim(' ');
                                break;
                            }
                        }
                    }

                    return callback(isConnecting, clientAddress, name);
                });

            byte[] silence = new byte[512];
            dummy.SubmitData(silence, silence.Length);

            encoder.Stream = Stream;
            dummy.Stop();

            IsStarted = Stream.Play();
            return IsStarted;
        }

        public void Stop()
        {
            if (IsStarted)
            {
                Stream?.Stop();
                Stream = null;
                encoder?.Stop();
                encoder = null;
                Device?.Shutdown();
                Device = null;
                server = null;
                callback = null;
                IsStarted = false;
            }
        }

        public bool Kick(string clientAddress)
        {
            if (server == null)
                return false;
            return server.Kick(clientAddress);
        }
    }
}

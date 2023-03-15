using ManagedBass;
using SoundFlux.Audio.Device;

namespace SoundFlux.Audio.Stream
{
    public class RecordStream : Stream
    {
        public const int DefaultRecordingPeriodMs = 100;

        public InputDevice Device { get; private set; }
        public int Period { get; private set; }

        public RecordStream(InputDevice device, int channels, int sampleRate, bool floatSamples = true,
            int period = DefaultRecordingPeriodMs, RecordProcedure? callback = null)
        {
            Device = device;
            RecordProcedure _callback = callback == null ? ((a, b, c, d) => true) : callback;
            Period = period;

            Device.Select();
            Handle = TryStart(channels, sampleRate, floatSamples, _callback);

            if (Handle == 0 && Bass.LastError == Errors.SampleFormat)
            {
                Handle = TryStart(channels, sampleRate, !floatSamples, _callback);
                if (Handle == 0)
                    throw new BassException();
            }
        }

        private int TryStart(int channels, int sampleRate, bool floatSamples, RecordProcedure callback)
            => Bass.RecordStart(sampleRate, channels,
                BassFlags.RecordPause | (floatSamples ? BassFlags.Float : 0), Period, callback);
    }
}

using ManagedBass;
using SoundFlux.Audio.Device;

namespace SoundFlux.Audio.Stream
{
    internal class DummyStream : Stream
    {
        public IAudioDevice Device { get; private set; }

        public DummyStream(IAudioDevice device, int sampleRate, int channels, bool floatSamples)
        {
            Device = device;
            device.Select();
            Handle = Bass.CreateStream(sampleRate, channels, BassFlags.Decode |
                (floatSamples ? BassFlags.Float : 0), StreamProcedureType.Dummy);
            if (Handle == 0)
                throw new BassException();
        }

        public override bool Play() => true;

        public override bool Pause() => true;

        public int SubmitData(byte[] data, int length)
            => Bass.ChannelGetData(Handle, data, length);
    }
}

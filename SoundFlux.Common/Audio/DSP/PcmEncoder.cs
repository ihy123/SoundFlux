using ManagedBass;
using ManagedBass.Enc;

namespace SoundFlux.Audio.DSP
{
    internal class PcmEncoder : Encoder
    {
        public override Stream.Stream? Stream
        {
            get => stream;
            set
            {
                if (value != null && value.Handle != stream?.Handle)
                {
                    stream = value;
                    if (!BassEnc.EncodeSetChannel(Handle, stream.Handle))
                        throw new BassException();
                }
            }
        }

        private Stream.Stream? stream;
        private EncodeProcedure callback;

        public PcmEncoder(Stream.Stream stream, EncodeProcedure? proc = null)
        {
            this.stream = stream;
            callback = proc == null ? ((a, b, c, d, e) => { }) : proc;
            Handle = BassEnc.EncodeStart(stream.Handle, null, EncodeFlags.PCM, callback);
            if (Handle == 0)
                throw new BassException();
        }

        public override bool Stop()
            => BassEnc.EncodeStop(Handle);
    }
}

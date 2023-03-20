namespace SoundFlux.Audio.DSP
{
    internal abstract class Encoder
    {
        public int Handle { get; protected set; }

        public abstract Stream.Stream Stream { get; set; }

        public abstract bool Stop();
    }
}

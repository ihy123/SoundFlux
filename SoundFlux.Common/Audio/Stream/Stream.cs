using ManagedBass;

namespace SoundFlux.Audio.Stream
{
    public abstract class Stream
    {
        public int Handle { get; protected set; }

        public bool FloatSamples
        {
            get
            {
                if (Bass.ChannelGetInfo(Handle, out ChannelInfo info))
                    return info.Resolution == Resolution.Float;
                return false;
            }
        }

        public int SampleRate
        {
            get
            {
                if (Bass.ChannelGetInfo(Handle, out ChannelInfo info))
                    return info.Frequency;
                return 0;
            }
        }

        public int Channels
        {
            get
            {
                if (Bass.ChannelGetInfo(Handle, out ChannelInfo info))
                    return info.Channels;
                return 0;
            }
        }

        // from 0 to 1
        public double Volume
        {
            get => Bass.ChannelGetAttribute(Handle, ChannelAttribute.Volume);
            set
            {
                if (!Bass.ChannelSetAttribute(Handle, ChannelAttribute.Volume, value))
                    throw new BassException();
            }
        }

        public virtual bool Play()
            => Bass.ChannelPlay(Handle, false);

        public virtual bool Pause()
            => Bass.ChannelPause(Handle);

        public virtual bool Stop()
            => Bass.ChannelStop(Handle);
    }
}

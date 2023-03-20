namespace SoundFlux.Audio.Device
{
    public interface IAudioDevice
    {
        public int Handle { get; }
        string? Name { get; }

        void Initialize();
        void Select();
        void Shutdown();
    }
}

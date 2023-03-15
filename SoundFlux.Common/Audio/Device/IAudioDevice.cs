namespace SoundFlux.Audio.Device
{
    public interface IAudioDevice
    {
        string? Name { get; }

        void Initialize();
        void Select();
        void Shutdown();
    }
}

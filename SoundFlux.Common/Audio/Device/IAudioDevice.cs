namespace SoundFlux.Audio.Device
{
    public interface IAudioDevice
    {
        int Handle { get; }
        string? Name { get; }

        void Initialize();
        void Select();
        void Shutdown();
    }
}

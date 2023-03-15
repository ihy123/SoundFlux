using Avalonia.Data.Converters;
using SoundFlux.Audio.Device;

namespace SoundFlux.ViewModels
{
    internal static class AudioDeviceConverter
    {
        public static readonly IValueConverter ToNameString =
            new FuncValueConverter<IAudioDevice, string>(dev =>
            {
                switch (dev?.Name)
                {
                    case "Default": return Resources.Resources.Default;
                    case "No sound": return Resources.Resources.NoSound;
                    case null: return Resources.Resources.Unnamed;
                }
                return dev.Name;
            });
    }
}

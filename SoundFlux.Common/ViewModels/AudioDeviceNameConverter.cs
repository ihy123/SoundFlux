using Avalonia.Data.Converters;

namespace SoundFlux.ViewModels
{
    internal static class AudioDeviceNameConverter
    {
        public static readonly IValueConverter Localize =
            new FuncValueConverter<string, string>(name =>
            {
                switch (name)
                {
                    case "Default": return Resources.Resources.Default;
                    case "No sound": return Resources.Resources.NoSound;
                    case null: return Resources.Resources.Unnamed;
                }
                return name;
            });
    }
}

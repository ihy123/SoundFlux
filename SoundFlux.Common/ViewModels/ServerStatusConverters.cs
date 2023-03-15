using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SoundFlux.ViewModels
{
    internal static class ServerStatusConverters
    {
        public static readonly IValueConverter IsNotStarted =
            new FuncValueConverter<ServerStatus, bool>(s => s == ServerStatus.NotStarted);

        public static readonly IValueConverter IsNotTerminating =
            new FuncValueConverter<ServerStatus, bool>(s => s != ServerStatus.Terminating);

        public static readonly IValueConverter ToColor =
            new FuncValueConverter<ServerStatus, IBrush>(s =>
            {
                switch (s)
                {
                    case ServerStatus.Started: return Brushes.GreenYellow;
                    case ServerStatus.Terminating:
                    case ServerStatus.Starting: return Brushes.Yellow;
                }
                return Brushes.Red;
            });
    }
}

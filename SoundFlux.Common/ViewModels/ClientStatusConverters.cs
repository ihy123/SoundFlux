using Avalonia.Data.Converters;
using Avalonia.Media;

namespace SoundFlux.ViewModels
{
    internal static class ClientStatusConverters
    {
        public static readonly IValueConverter IsNotConnected =
            new FuncValueConverter<ClientStatus, bool>(s => s == ClientStatus.NotConnected);

        public static readonly IValueConverter IsNotDisconnecting =
            new FuncValueConverter<ClientStatus, bool>(s => s != ClientStatus.Disconnecting);

        public static readonly IValueConverter ToColor =
            new FuncValueConverter<ClientStatus, IBrush>(s =>
            {
                switch (s)
                {
                    case ClientStatus.Connected: return Brushes.GreenYellow;
                    case ClientStatus.Disconnecting:
                    case ClientStatus.Connecting: return Brushes.Yellow;
                }
                return Brushes.Red;
            });
    }
}

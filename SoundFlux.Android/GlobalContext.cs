using SoundFlux.Services;

namespace SoundFlux.Android
{
    internal static class GlobalContext
    {
        public static Client Client => client;
        public static Server Server => server;

        private static Client client = new();
        private static Server server = new();
    }
}

using SoundFlux.Services.Dummy;

namespace SoundFlux.Services
{
    public static class ServiceRegistry
    {
        public static SettingsManager SettingsManager
        {
            get => _settingsManager ??= new DummySettingsManager();
            set => _settingsManager = value;
        }

        //public static Client Client
        //{
        //    get => _client ??= new Client();
        //    set => _client = value;
        //}

        //public static Server Server
        //{
        //    get => _server ??= new Server();
        //    set => _server = value;
        //}

        public static IErrorHandler ErrorHandler
        {
            get => _errorHandler ??= new DummyErrorHandler();
            set => _errorHandler = value;
        }

        public static INetHelper NetHelper
        {
            get => _netHelper ??= new DummyNetHelper();
            set => _netHelper = value;
        }

        private static SettingsManager? _settingsManager;
        private static IErrorHandler? _errorHandler;
        private static INetHelper? _netHelper;
        //private static Client? _client;
        //private static Server? _server;
    }
}

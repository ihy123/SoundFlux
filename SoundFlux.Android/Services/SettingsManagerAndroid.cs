using SoundFlux.Services;

namespace SoundFlux.Android.Services
{
    internal class SettingsManagerAndroid : SettingsManager
    {
        public override string SettingsDirectory => configPath;

        private string configPath;

        public SettingsManagerAndroid(string configPath)
            => this.configPath = configPath;
    }
}

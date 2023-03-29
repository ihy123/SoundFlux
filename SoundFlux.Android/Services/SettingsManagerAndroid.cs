using Android.App;
using SoundFlux.Services;

namespace SoundFlux.Android.Services
{
    internal class SettingsManagerAndroid : SettingsManager
    {
        public override string SettingsDirectory
            => Application.Context!.FilesDir!.AbsolutePath + '/';
    }
}

using SoundFlux.Services;
using System;

namespace SoundFlux.Desktop.Services
{
    internal class SettingsManagerWin32 : SettingsManager
    {
        public override string SettingsDirectory => Environment.ExpandEnvironmentVariables("%AppData%\\SoundFlux\\");
    }
}

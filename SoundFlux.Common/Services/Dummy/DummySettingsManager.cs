using System;

namespace SoundFlux.Services.Dummy
{
    internal class DummySettingsManager : SettingsManager
    {
        public override string SettingsDirectory => string.Empty;

        public override void Load()
            => throw new NotImplementedException();

        public override void Save()
            => throw new NotImplementedException();
    }
}

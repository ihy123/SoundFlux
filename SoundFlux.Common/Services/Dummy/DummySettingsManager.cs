using System;

namespace SoundFlux.Services.Dummy
{
    internal class DummySettingsManager : SettingsManager
    {
        public override string SettingsDirectory => string.Empty;

        public override bool Load()
            => throw new NotImplementedException();

        public override void Save()
            => throw new NotImplementedException();
    }
}

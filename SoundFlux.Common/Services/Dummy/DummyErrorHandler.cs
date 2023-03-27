using System;

namespace SoundFlux.Services.Dummy
{
    internal class DummyErrorHandler : IErrorHandler
    {
        public void Error(string message)
            => throw new NotImplementedException();
    }
}

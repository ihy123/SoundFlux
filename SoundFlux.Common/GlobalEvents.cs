using System;

namespace SoundFlux
{
    public static class GlobalEvents
    {
        public static event Action? OnExitEvent;
        public static void OnExit() { OnExitEvent?.Invoke(); }

        public static event Action<string>? OnErrorEvent;
        public static void OnError(string message) { OnErrorEvent?.Invoke(message); }
    }
}

using Android.App;
using Android.Content;
using Android.OS;

namespace SoundFlux.Android
{
    [Service]
    public class PlayerService : Service
    {
        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }
    }
}

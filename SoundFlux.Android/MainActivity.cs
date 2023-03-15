using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;

namespace SoundFlux.Android
{
    [Activity(Label = "SoundFlux.Android", Theme = "@style/MyTheme.NoActionBar", Icon = "@drawable/icon", LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AvaloniaMainActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //BindService(new Intent(Application.Context, typeof(MainActivity)), null, Bind.AutoCreate);
        }

        protected override void OnStop()
        {
            base.OnStop();
            GlobalContext.OnExit();
            SharedSettings.Instance.Save();
        }
    }
}

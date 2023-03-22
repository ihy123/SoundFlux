using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Avalonia.Android;
using System;

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
            try
            {
                GlobalContext.OnExit();
                SharedSettings.Instance.Save();
            }
            catch (Exception e)
            {
                Log.Error("MainActivity", e.ToString());
            }
        }
    }
}

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using AndroidX.Core.App;
using Avalonia;
using Avalonia.Android;
using SoundFlux.Android.Services;
using SoundFlux.Services;
using System;
using Application = Android.App.Application;

namespace SoundFlux.Android
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AvaloniaSplashActivity<App>
    {
        protected override AppBuilder CreateAppBuilder()
        {
            try
            {
                var sm = new SettingsManagerAndroid(ApplicationContext!.FilesDir!.AbsolutePath + '/');
                ServiceRegistry.SettingsManager = sm;
                ServiceRegistry.NetHelper = new NetHelper();
                ServiceRegistry.ErrorHandler = new AlertDialogErrorHandler(ApplicationContext);
                sm.Load();
            }
            catch (Exception e)
            {
                Log.Error("SplashActivity", e.ToString());
            }
            return base.CreateAppBuilder();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == 101)
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
            else
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.RecordAudio }, 101);
        }
    }
}

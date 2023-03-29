using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
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
    public class SplashActivity : AvaloniaSplashActivity
    {
        protected override AppBuilder CreateAppBuilder()
        {
            ServiceRegistry.ErrorHandler = new AlertDialogErrorHandler();
            ServiceRegistry.SettingsManager = new SettingsManagerAndroid();
            ServiceRegistry.NetHelper = new NetHelper();

            try
            {
                ServiceRegistry.SettingsManager.Load();
                GlobalContext.Client.LoadSettings();
                GlobalContext.Server.LoadSettings();
            }
            catch (Exception e)
            {
                ServiceRegistry.ErrorHandler.Error(e.ToString());
            }

            return AppBuilder.Configure(() => new App(
                GlobalContext.Client, GlobalContext.Server)).UseAndroid();
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

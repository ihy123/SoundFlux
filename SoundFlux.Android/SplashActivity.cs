using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using Avalonia;
using Avalonia.Android;
using Application = Android.App.Application;

namespace SoundFlux.Android
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AvaloniaSplashActivity<App>
    {
        protected override AppBuilder CreateAppBuilder()
        {
            new PlatformUtilsAndroid(ApplicationContext.FilesDir.AbsolutePath);
            SharedSettings.Instance.Load();
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
                RequestPermissions();
        }

        private void RequestPermissions()
            => ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.RecordAudio }, 101);
    }
}

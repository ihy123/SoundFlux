using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Avalonia.Android;
using SoundFlux.Android.Services;
using SoundFlux.Services;
using System;

namespace SoundFlux.Android
{
    [Activity(Label = "SoundFlux", Theme = "@style/MyTheme.NoActionBar", Icon = "@mipmap/ic_launcher", LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AvaloniaMainActivity
    {
        private class MainAlertBroadcastReceiver : BroadcastReceiver
        {
            private Context ctx;

            public MainAlertBroadcastReceiver(Context context)
                => ctx = context;

            public override void OnReceive(Context? context, Intent? intent)
            {
                new AlertDialog.Builder(ctx)
                    .SetCancelable(true)?
                    .SetTitle("Exception")?
                    .SetPositiveButton("OK", (IDialogInterfaceOnClickListener?)null)?
                    .SetMessage(intent?.GetStringExtra(
                        AlertDialogErrorHandler.AlertIntentMessageName))?
                    .Create()?.Show();

                InvokeAbortBroadcast();
            }
        }

        private MainAlertBroadcastReceiver? receiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            receiver = new(this);
            Application.Context.RegisterReceiver(receiver, new IntentFilter(
                AlertDialogErrorHandler.AlertBroadcastAction));

            //BindService(new Intent(Application.Context, typeof(MainActivity)), null, Bind.AutoCreate);
        }

        protected override void OnStop()
        {
            base.OnStop();

            try
            {
                GlobalContext.Client.SaveSettings();
                GlobalContext.Server.SaveSettings();

                App? app = (App?)Avalonia.Application.Current;
                app?.SaveSettings();

                ServiceRegistry.SettingsManager.Save();
            }
            catch (Exception e)
            {
                ServiceRegistry.ErrorHandler.Error(e.ToString());
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Application.Context.UnregisterReceiver(receiver);
            receiver = null;
        }
    }
}

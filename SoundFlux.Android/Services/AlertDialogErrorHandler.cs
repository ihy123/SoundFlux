using Android.App;
using Android.Content;
using Android.Util;
using SoundFlux.Services;

namespace SoundFlux.Android.Services
{
    internal class AlertDialogErrorHandler : IErrorHandler
    {
        private class DefaultAlertBroadcastReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context? context, Intent? intent)
            {
                Intent alert = new(Application.Context, typeof(AlertActivity));
                alert.PutExtra(AlertIntentMessageName,
                    intent?.GetStringExtra(AlertIntentMessageName));
                alert.AddFlags(ActivityFlags.NewTask);

                Application.Context.StartActivity(alert);

                InvokeAbortBroadcast();
            }
        }

        public const string AlertBroadcastAction = "AlertDialogErrorHandler:BROADCAST";
        public const string AlertIntentMessageName = "message";

        private DefaultAlertBroadcastReceiver receiver = new();

        public void Error(string message)
        {
            Log.Error(nameof(AlertDialogErrorHandler), message);

            var intent = new Intent(AlertBroadcastAction);
            intent.PutExtra(AlertIntentMessageName, message);
            Application.Context.SendOrderedBroadcast(intent, null);
        }

        public AlertDialogErrorHandler()
        {
            Application.Context.RegisterReceiver(
                receiver, new IntentFilter(AlertBroadcastAction)
                {
                    Priority = 1
                });
        }

        ~AlertDialogErrorHandler()
            => Application.Context.UnregisterReceiver(receiver);
    }
}

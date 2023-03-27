using Android.App;
using Android.Content;
using Android.Util;
using SoundFlux.Services;

namespace SoundFlux.Android.Services
{
    internal class AlertDialogErrorHandler : IErrorHandler
    {
        private AlertDialog? dialog;

        public void Error(string message)
        {
            Log.Error(nameof(AlertDialogErrorHandler), message);
            dialog?.SetMessage(message);
            dialog?.Show();
        }

        public AlertDialogErrorHandler(Context appContext)
        {
            dialog = new AlertDialog.Builder(appContext)
                .SetCancelable(true)?
                .SetTitle("Exception")?
                .SetPositiveButton("OK", (IDialogInterfaceOnClickListener?)null)?
                .Create();
        }
    }
}

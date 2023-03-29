using Android.App;
using Android.Content;
using AndroidX.AppCompat.App;
using SoundFlux.Android.Services;
using AlertDialog = Android.App.AlertDialog;

namespace SoundFlux.Android
{
    [Activity(Label = "SoundFlux.Alert", Theme = "@style/MyTheme.NoActionBar")]
    public class AlertActivity : AppCompatActivity
    {
        protected override void OnResume()
        {
            base.OnResume();

            new AlertDialog.Builder(this)
                .SetCancelable(true)?
                .SetTitle("Exception")?
                .SetPositiveButton("OK", (IDialogInterfaceOnClickListener?)null)?
                .SetMessage(Intent?.GetStringExtra(AlertDialogErrorHandler.AlertIntentMessageName))?
                .Create()?.Show();
        }
    }
}

using Android.App;
using Android.Content;
using Android.Gms.Gcm;

namespace PushNotification.Plugin
{
    [BroadcastReceiver(Permission = "com.google.android.c2dm.permission.SEND", Exported=true)]
    [IntentFilter(new string[] { "com.google.android.c2dm.intent.RECEIVE" }, Categories = new string[] { "@PACKAGE_NAME@" })]
    public class PushNotificationsReceiver : GcmReceiver
    {
        /*public override void OnReceive(Context context, Intent intent)
        {
          
			var serviceIntent = new Intent(context,typeof(PushNotificationGcmListener));
            serviceIntent.AddFlags(ActivityFlags.IncludeStoppedPackages);
            serviceIntent.ReplaceExtras(intent.Extras);
            serviceIntent.SetAction( intent.Action);
            StartWakefulService(context, serviceIntent);
          

            ResultCode = Result.Ok;
        }*/
    }
}
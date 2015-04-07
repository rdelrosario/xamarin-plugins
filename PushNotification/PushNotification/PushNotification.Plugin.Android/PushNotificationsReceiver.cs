using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Support.V4.Content;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

[assembly: Permission(Name = "@PACKAGE_NAME@.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]

namespace PushNotification.Plugin
{
    [BroadcastReceiver(Permission = "com.google.android.c2dm.permission.SEND")]
    [IntentFilter(new string[] { "com.google.android.c2dm.intent.RECEIVE" }, Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { "com.google.android.c2dm.intent.REGISTRATION" }, Categories = new string[] { "@PACKAGE_NAME@" })]
    [IntentFilter(new string[] { "com.google.android.gcm.intent.RETRY" }, Categories = new string[] { "@PACKAGE_NAME@" })]
    public class PushNotificationsReceiver : WakefulBroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
          
			var serviceIntent = new Intent(context,typeof(PushNotificationImplementation));
            serviceIntent.AddFlags(ActivityFlags.IncludeStoppedPackages);
            serviceIntent.ReplaceExtras(intent.Extras);
            serviceIntent.SetAction( intent.Action);
            StartWakefulService(context, serviceIntent);
          

            ResultCode = Result.Ok;
        }
    }
}
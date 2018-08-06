using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PushNotification.Plugin
{
    //[BroadcastReceiver]
    //[IntentFilter(new[] { "android.intent.action.MY_PACKAGE_REPLACED" })]
    public class AppUpdateReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (CrossPushNotification.IsInitialized)
                CrossPushNotification.Current.Register();
        }
    }
}
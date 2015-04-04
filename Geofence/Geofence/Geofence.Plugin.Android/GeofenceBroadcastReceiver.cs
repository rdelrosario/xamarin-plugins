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
using Android.Support.V4.Content;

[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
namespace Geofence.Plugin
{
    [BroadcastReceiver]
    [IntentFilter(new string[] { "@PACKAGE_NAME@.ACTION_RECEIVE_GEOFENCE" })]
    public class GeofenceBroadcastReceiver : WakefulBroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}",CrossGeofence.Tag,"Region State Change Received"));
            var serviceIntent = new Intent(context, typeof(GeofenceImplementation));
            serviceIntent.AddFlags(ActivityFlags.IncludeStoppedPackages);
            serviceIntent.ReplaceExtras(intent.Extras);
            serviceIntent.SetAction(intent.Action);
            StartWakefulService(context, serviceIntent);


            ResultCode = Result.Ok;
        }
    }
}
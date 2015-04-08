using Android.App;
using Android.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin
{
    /// <summary>
    /// GeofenceBootReceiver class
    /// Receives BOOT_COMPLETED event
    /// </summary>
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted})]
    public class GeofenceBootReceiver : BroadcastReceiver
    {
        /// <summary>
        /// On boot completed restores all persisted regions
        /// </summary>
        /// <param name="context"></param>
        /// <param name="intent"></param>
        public override void OnReceive(Context context, Intent intent)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, "Boot Received"));
            var geofenceInstance= CrossGeofence.Current;
        }
    }
}

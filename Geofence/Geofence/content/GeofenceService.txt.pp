using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace $rootnamespace$.Helpers
{
    [Service]
    public class GeofenceService : Service
    {
        public override void OnCreate()
        {
            base.OnCreate();

            System.Diagnostics.Debug.WriteLine("Geofence Service - Created");
        }

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            System.Diagnostics.Debug.WriteLine("Geofence Service - Started");
            return StartCommandResult.Sticky;
        }

        public override Android.OS.IBinder OnBind(Android.Content.Intent intent)
        {
            System.Diagnostics.Debug.WriteLine("Geofence Service - Binded");
            return null;
        }

        public override void OnDestroy()
        {
            System.Diagnostics.Debug.WriteLine("Geofence Service - Destroyed");
            base.OnDestroy();
        }
    }
}

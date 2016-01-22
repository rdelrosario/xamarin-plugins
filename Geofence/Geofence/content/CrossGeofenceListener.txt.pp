using Geofence.Plugin;
using Geofence.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace $rootnamespace$.Helpers
{
   //Class to handle geofence events such as start/stop monitoring, region state changes and errors.
    public class CrossGeofenceListener : IGeofenceListener
    {
       public void OnMonitoringStarted(string region)
        {
	    Debug.WriteLine(string.Format("{0} - {1}: {2}", CrossGeofence.Id, "Monitoring in region",region));
        }

        public void OnMonitoringStopped()
        {
            Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, "Monitoring stopped for all regions"));
        }

        public void OnMonitoringStopped(string identifier)
        {
            Debug.WriteLine(string.Format("{0} - {1}: {2}", CrossGeofence.Id, "Monitoring stopped in region", identifier));
        }

        public void OnError(string error)
        {
            Debug.WriteLine(string.Format("{0} - {1}: {2}", CrossGeofence.Id, "Error", error));
        }


        public void OnRegionStateChanged(GeofenceResult result)
        {
            Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, result.ToString()));
        }

        // Note that you must call CrossGeofence.GeofenceListener.OnAppStarted() from your app when you want this method to run.
        public void OnAppStarted()
        {
            Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Tag, "App started"));
        }
    }
}

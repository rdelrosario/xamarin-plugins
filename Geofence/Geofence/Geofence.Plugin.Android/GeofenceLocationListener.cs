using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin
{
    /// <summary>
    /// GeofenceLocationListener class
    /// Listens to location updates
    /// </summary>
    public class GeofenceLocationListener : Java.Lang.Object,Android.Gms.Location.ILocationListener
    {
        private static GeofenceLocationListener sharedInstance = new GeofenceLocationListener();

        public static GeofenceLocationListener SharedInstance { get { return sharedInstance; } }
        
        private GeofenceLocationListener()
        {

        }
        void Android.Gms.Location.ILocationListener.OnLocationChanged(Android.Locations.Location location)
        {
            //Location Updated
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2},{3}",CrossGeofence.Id,"Location Update",location.Latitude,location.Longitude));
        }
    }
}

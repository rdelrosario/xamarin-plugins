using Geofence.Plugin.Abstractions;
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

        /// <summary>
        /// Location listener instance
        /// </summary>
        public static GeofenceLocationListener SharedInstance { get { return sharedInstance; } }
        
        private GeofenceLocationListener()
        {

        }
        void Android.Gms.Location.ILocationListener.OnLocationChanged(Android.Locations.Location location)
        {
            //Location Updated
            var currentGeofenceImplementation = CrossGeofence.Current as GeofenceImplementation; 
                  
            // Check if we need to reset the listener in case there was an error, e.g. location services turned off
            if (currentGeofenceImplementation.LocationHasError)
            {
                // Reset the broadcast receiver here
                currentGeofenceImplementation.AddGeofences();

                // Reset
                currentGeofenceImplementation.LocationHasError = false;
            }
      
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2},{3}",CrossGeofence.Id,"Location Update",location.Latitude,location.Longitude));
            currentGeofenceImplementation.SetLastKnownLocation(location);
            
        }
    }
}

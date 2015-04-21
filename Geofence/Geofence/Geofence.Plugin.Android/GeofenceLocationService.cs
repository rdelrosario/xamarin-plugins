using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin
{
    /// <summary>
    /// GeofenceLocationService
    /// </summary>
    [Service]
    public class GeofenceLocationService : Service, Android.Gms.Location.ILocationListener
    {
        /// <summary>
        /// Location Service OnCreate method
        /// </summary>
        public override void OnCreate()
        {
           /* mLocationRequest = Android.Gms.Location.LocationRequest.create();
            mLocationRequest.setInterval(CommonUtils.UPDATE_INTERVAL_IN_MILLISECONDS);
            mLocationRequest.setPriority(LocationRequest.PRIORITY_HIGH_ACCURACY);
            mLocationRequest.setFastestInterval(CommonUtils.FAST_INTERVAL_CEILING_IN_MILLISECONDS);
            mLocationClient = new LocationClient(getApplicationContext(), this, this);
            mLocationClient.connect();*/
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public override Android.OS.IBinder OnBind(Android.Content.Intent intent)
        {
          return null;
        }


        /// <summary>
        /// Location changed method
        /// </summary>
        /// <param name="location"></param>
        public void OnLocationChanged(Android.Locations.Location location)
        {
            //Location Updated
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2},{3}", CrossGeofence.Id, "Location Update", location.Latitude, location.Longitude));
       
        }
    }
}

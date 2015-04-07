using Android.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin
{
    [Service]
    public class GeofenceLocationService : Service, Android.Gms.Location.ILocationListener
    {
        public override void OnCreate()
        {
           /* mLocationRequest = Android.Gms.Location.LocationRequest.create();
            mLocationRequest.setInterval(CommonUtils.UPDATE_INTERVAL_IN_MILLISECONDS);
            mLocationRequest.setPriority(LocationRequest.PRIORITY_HIGH_ACCURACY);
            mLocationRequest.setFastestInterval(CommonUtils.FAST_INTERVAL_CEILING_IN_MILLISECONDS);
            mLocationClient = new LocationClient(getApplicationContext(), this, this);
            mLocationClient.connect();*/
        }

        public override Android.OS.IBinder OnBind(Android.Content.Intent intent)
        {
          return null;
        }



        public void OnLocationChanged(Android.Locations.Location location)
        {
            //Location Updated
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2},{3}", CrossGeofence.Tag, "Location Update", location.Latitude, location.Longitude));
       
        }
    }
}

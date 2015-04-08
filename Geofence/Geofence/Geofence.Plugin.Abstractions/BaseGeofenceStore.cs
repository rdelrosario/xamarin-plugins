using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    public abstract class BaseGeofenceStore : IGeofenceStore
    {
        protected const string GeofenceStoreId = "CrossGeofence.Store";
        protected const string LatitudeGeofenceRegionKey = "Geofence.Region.Latitude";
        protected const string LongitudeGeofenceRegionKey = "Geofence.Region.Longitude";
        protected const string RadiusGeofenceRegionKey = "Geofence.Region.Radius";
        protected const string IdGeofenceRegionKey = "Geofence.Region.Id";
        protected const string TransitionTypeGeofenceRegionKey = "Geofence.Region.TransitionType";
        protected const string ExpirationDurationGeofenceRegionKey = "Geofence.Region.ExpirationDuration";
        protected const string NotifyOnEntryGeofenceRegionKey = "Geofence.Region.NotifyOnEntry";
        protected const string NotifyOnExitGeofenceRegionKey = "Geofence.Region.NotifyOnExit";
        protected const string NotifyOnStayGeofenceRegionKey = "Geofence.Region.NotifyOnStay";
        protected const string NotificationEntryMessageGeofenceRegionKey = "Geofence.Region.NotificationEntryMessage";
        protected const string NotificationExitMessageGeofenceRegionKey = "Geofence.Region.NotificationExitMessage";
        protected const string NotificationStayMessageGeofenceRegionKey = "Geofence.Region.NotificationStayMessage";
        /// <summary>
        /// Given a Geofence object's ID and the name of a field , return the keyname of the object's values in Store
        /// </summary>
        /// <returns>The full key name o a value in SharedPreferences</returns>
        /// <param name="id">The ID of a Geofence object</param>
        /// <param name="fieldName">The field represented by the key</param>
        protected string GetGeofenceFieldKey(String id, String fieldName)
        {
            return GeofenceStoreId + "_" + id + "_" + fieldName;
        }

        public abstract  Dictionary<string, GeofenceCircularRegion> GetGeofenceRegions();
        public abstract GeofenceCircularRegion GetGeofenceRegion(string id);
        public abstract void SetGeofenceRegion(GeofenceCircularRegion region);
        public abstract void ClearGeofenceRegions();
        public abstract void RemoveGeofenceRegion(String id);
        
    }
}

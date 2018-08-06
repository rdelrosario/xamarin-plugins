using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    /// <summary>
    /// BaseGeofence Store abstract class
    /// </summary>
    public abstract class BaseGeofenceStore : IGeofenceStore
    {
        /// <summary>
        /// Storage Keys
        /// </summary>
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
        protected const string ShowNotificationGeofenceRegionKey = "Geofence.Region.ShowNotification";
        protected const string PersistentGeofenceRegionKey = "Geofence.Region.Persistent";
        protected const string ShowEntryNotificationGeofenceRegionKey = "Geofence.Region.ShowEntryNotification";
        protected const string ShowExitNotificationGeofenceRegionKey = "Geofence.Region.ShowExitNotification";
        protected const string ShowStayNotificationGeofenceRegionKey = "Geofence.Region.ShowStayNotification";
        protected const string StayedInThresholdDurationGeofenceRegionKey = "Geofence.Region.StayedInThresholdDuration";
        protected const string ExitThresholdDurationGeofenceRegionKey = "Geofence.Region.ExitThresholdDuration";
        /// <summary>
        /// Given a GeofenceCircularRegion object's ID and the name of a field , return the keyname of the object's values in Store
        /// </summary>
        /// <returns>The full key name o a value in Store</returns>
        /// <param name="id">The ID of a GeofenceCircularRegion object</param>
        /// <param name="fieldName">The field represented by the key</param>
        protected string GetFieldKey(string id, string fieldName)
        {
            return GeofenceStoreId + "_" + id + "_" + fieldName;
        }
        /// <summary>
        /// Gets all stored geofence regions
        /// </summary>
        /// <returns></returns>
        public abstract  Dictionary<string, GeofenceCircularRegion> GetAll();
        /// <summary>
        /// Gets specific geofence region from store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract GeofenceCircularRegion Get(string id);
        /// <summary>
        /// Saves geofence region in store
        /// </summary>
        /// <param name="region"></param>
        public abstract void Save(GeofenceCircularRegion region);
        /// <summary>
        /// Clear geofence regions in store
        /// </summary>
        public abstract void RemoveAll();
        /// <summary>
        /// Remove specific geofence region from store
        /// </summary>
        /// <param name="id"></param>
        public abstract void Remove(String id);
        
    }
}

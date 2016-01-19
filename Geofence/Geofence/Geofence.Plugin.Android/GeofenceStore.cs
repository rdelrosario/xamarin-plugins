using Android.Content;
using Geofence.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin
{
    internal class GeofenceStore : BaseGeofenceStore
    {
        // The SharedPreferences object in which geofences are stored
		readonly ISharedPreferences mPrefs;

        // Invalid values, used to test geofence storage when retrieving geofences.
        const long InvalidLongValue = -999L;
        const float InvalidFloatValue= -999.0f;
        const int InvalidIntValue= -999;

        private static GeofenceStore sharedInstance=new GeofenceStore();

        public static GeofenceStore SharedInstance { get { return sharedInstance; } }

		/// <summary>
		/// Create the SharedPreferences storage with private access only.
		/// </summary>
		private GeofenceStore ()
		{
            mPrefs = Android.App.Application.Context.GetSharedPreferences(GeofenceStoreId, FileCreationMode.Private);
		}
 

		/// <summary>
		/// Returns a stored geofence by its ID, or returns null if it's not found
		/// </summary>
		/// <returns>A SimpleGeofence defined by its center and radius, or null if the ID is invalid</returns>
		/// <param name="id">The ID of a stored Geofence</param>
		public override GeofenceCircularRegion Get(String id)
		{

			// Get the latitude for the geofence identified by id, or INVALID_FLOAT_VALUE if it doesn't exist (similarly for the other values that follow)
            double lat = mPrefs.GetFloat(GetFieldKey(id, LatitudeGeofenceRegionKey), InvalidFloatValue);
            double lng = mPrefs.GetFloat(GetFieldKey(id, LongitudeGeofenceRegionKey), InvalidFloatValue);
            double radius = mPrefs.GetFloat(GetFieldKey(id, RadiusGeofenceRegionKey), InvalidFloatValue);
            bool notifyOnEntry = mPrefs.GetBoolean(GetFieldKey(id, NotifyOnEntryGeofenceRegionKey), false);
            bool notifyOnExit = mPrefs.GetBoolean(GetFieldKey(id, NotifyOnExitGeofenceRegionKey), false);
            bool notifyOnStay = mPrefs.GetBoolean(GetFieldKey(id, NotifyOnStayGeofenceRegionKey), false);
            string notificationEntryMessage = mPrefs.GetString(GetFieldKey(id, NotificationEntryMessageGeofenceRegionKey), string.Empty);
            string notificationExitMessage = mPrefs.GetString(GetFieldKey(id, NotificationExitMessageGeofenceRegionKey), string.Empty);
            string notificationStayMessage = mPrefs.GetString(GetFieldKey(id, NotificationStayMessageGeofenceRegionKey), string.Empty);
            bool showNotification = mPrefs.GetBoolean(GetFieldKey(id, ShowNotificationGeofenceRegionKey), false);
            bool persistent = mPrefs.GetBoolean(GetFieldKey(id, PersistentGeofenceRegionKey), false);
            bool showEntryNotification = mPrefs.GetBoolean(GetFieldKey(id, ShowEntryNotificationGeofenceRegionKey), false);
            bool showExitNotification = mPrefs.GetBoolean(GetFieldKey(id, ShowExitNotificationGeofenceRegionKey), false);
            bool showStayNotification = mPrefs.GetBoolean(GetFieldKey(id, ShowStayNotificationGeofenceRegionKey), false);
            int stayedInThresholdDuration = mPrefs.GetInt(GetFieldKey(id, StayedInThresholdDurationGeofenceRegionKey), InvalidIntValue);
            //long expirationDuration = mPrefs.GetLong(GetFieldKey(id, ExpirationDurationGeofenceRegionKey), InvalidLongValue);
            //int transitionType = mPrefs.GetInt(GetFieldKey(id, TransitionTypeGeofenceRegionKey), InvalidIntValue);

			// If none of the values is incorrect, return the object
            if (lat != InvalidFloatValue
                && lng != InvalidFloatValue
                && radius != InvalidFloatValue
                && persistent
                && stayedInThresholdDuration != InvalidIntValue
                // && expirationDuration != InvalidLongValue
                //&& transitionType != InvalidIntValue
                )
                return new GeofenceCircularRegion(id,lat,lng,radius,notifyOnEntry,notifyOnExit,notifyOnStay,showNotification,persistent,
                                                  showEntryNotification, showExitNotification, showStayNotification)
                {

                    NotificationEntryMessage=notificationEntryMessage,
                    NotificationStayMessage=notificationStayMessage,
                    NotificationExitMessage=notificationExitMessage,
                   
                    StayedInThresholdDuration = TimeSpan.FromMilliseconds(stayedInThresholdDuration)

                };

			// Otherwise return null
			return null;
		}

		/// <summary>
		/// Save a geofence
		/// </summary>
		/// <param name="region">The GeofenceCircularRegion with the values you want to save in SharedPreferemces</param>
		public override void Save(GeofenceCircularRegion region) {

            if (!region.Persistent)
                return;

            string id = region.Id;
			// Get a SharedPreferences editor instance. Among other things, SharedPreferences ensures that updates are atomic and non-concurrent
			ISharedPreferencesEditor prefs = mPrefs.Edit();
			// Write the geofence values to SharedPreferences 
            prefs.PutFloat(GetFieldKey(id, LatitudeGeofenceRegionKey), (float)region.Latitude);
            prefs.PutFloat(GetFieldKey(id, LongitudeGeofenceRegionKey), (float)region.Longitude);
            prefs.PutFloat(GetFieldKey(id, RadiusGeofenceRegionKey), (float)region.Radius);
            prefs.PutBoolean(GetFieldKey(id, NotifyOnEntryGeofenceRegionKey), region.NotifyOnEntry);
            prefs.PutBoolean(GetFieldKey(id, NotifyOnExitGeofenceRegionKey), region.NotifyOnExit);
            prefs.PutBoolean(GetFieldKey(id, NotifyOnStayGeofenceRegionKey), region.NotifyOnStay);
            prefs.PutString(GetFieldKey(id, NotificationEntryMessageGeofenceRegionKey), region.NotificationEntryMessage);
            prefs.PutString(GetFieldKey(id, NotificationExitMessageGeofenceRegionKey), region.NotificationExitMessage);
            prefs.PutString(GetFieldKey(id, NotificationStayMessageGeofenceRegionKey), region.NotificationStayMessage);
            prefs.PutBoolean(GetFieldKey(id, ShowNotificationGeofenceRegionKey), region.ShowNotification);
            prefs.PutBoolean(GetFieldKey(id, PersistentGeofenceRegionKey), region.Persistent);
            prefs.PutBoolean(GetFieldKey(id, ShowEntryNotificationGeofenceRegionKey), region.ShowEntryNotification);
            prefs.PutBoolean(GetFieldKey(id, ShowExitNotificationGeofenceRegionKey), region.ShowExitNotification);
            prefs.PutBoolean(GetFieldKey(id, ShowStayNotificationGeofenceRegionKey), region.ShowStayNotification);
            prefs.PutInt(GetFieldKey(id, StayedInThresholdDurationGeofenceRegionKey), (int)region.StayedInThresholdDuration.TotalMilliseconds);
			// Commit the changes
			prefs.Commit ();
           
		}

        public override void RemoveAll()
        {
            ISharedPreferencesEditor prefs = mPrefs.Edit();
            prefs.Clear();
            // Commit the changes
            prefs.Commit();
        }

        public override void Remove(string id)
        {
            try
            {
                ISharedPreferencesEditor prefs = mPrefs.Edit();
         
                prefs.Remove(GetFieldKey(id, LatitudeGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, LongitudeGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, RadiusGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, NotifyOnEntryGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, NotifyOnExitGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, NotifyOnStayGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, NotificationEntryMessageGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, NotificationExitMessageGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, NotificationStayMessageGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, ShowNotificationGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, PersistentGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, ShowEntryNotificationGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, ShowExitNotificationGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, ShowStayNotificationGeofenceRegionKey));
                prefs.Remove(GetFieldKey(id, StayedInThresholdDurationGeofenceRegionKey));
                // Commit the changes
                prefs.Commit();

            }catch(Java.Lang.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Error: {1}", CrossGeofence.Id, ex.ToString()));
            }
            
        }

        public override Dictionary<string,GeofenceCircularRegion> GetAll()
        {
            IEnumerable<string> keys = mPrefs.All.Where(p => p.Key.ToString().StartsWith(GeofenceStoreId) && p.Key.Split('_').Length > 1).Select(p => p.Key.Split('_')[1]).Distinct();

            Dictionary<string, GeofenceCircularRegion> regions = new Dictionary<string,GeofenceCircularRegion>();

            foreach (string key in keys)
            {
        
              
                var region = Get(key);
                if (region != null)
                {
                   
                    regions.Add(region.Id, region);
                }
           
                
            }
           
            return regions;
            
        }

		
    }
}

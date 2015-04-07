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
		/// <param name="context">Context.</param>
		private GeofenceStore ()
		{
            mPrefs = Android.App.Application.Context.GetSharedPreferences(GeofenceStoreId, FileCreationMode.Private);
		}
 

		/// <summary>
		/// Returns a stored geofence by its ID, or returns null if it's not found
		/// </summary>
		/// <returns>A SimpleGeofence defined by its center and radius, or null if the ID is invalid</returns>
		/// <param name="id">The ID of a stored Geofence</param>
		public override GeofenceCircularRegion GetGeofenceRegion(String id)
		{

			// Get the latitude for the geofence identified by id, or INVALID_FLOAT_VALUE if it doesn't exist (similarly for the other values that follow)
            double lat = mPrefs.GetFloat(GetGeofenceFieldKey(id, LatitudeGeofenceRegionKey), InvalidFloatValue);
            double lng = mPrefs.GetFloat(GetGeofenceFieldKey(id, LongitudeGeofenceRegionKey), InvalidFloatValue);
            double radius = mPrefs.GetFloat(GetGeofenceFieldKey(id, RadiusGeofenceRegionKey), InvalidFloatValue);
            bool notifyOnEntry = mPrefs.GetBoolean(GetGeofenceFieldKey(id, NotifyOnEntryGeofenceRegionKey), false);
            bool notifyOnExit = mPrefs.GetBoolean(GetGeofenceFieldKey(id, NotifyOnExitGeofenceRegionKey), false);
            //long expirationDuration = mPrefs.GetLong(GetGeofenceFieldKey(id, ExpirationDurationGeofenceRegionKey), InvalidLongValue);
            //int transitionType = mPrefs.GetInt(GetGeofenceFieldKey(id, TransitionTypeGeofenceRegionKey), InvalidIntValue);

			// If none of the values is incorrect, return the object
            if (lat != InvalidFloatValue
                && lng != InvalidFloatValue
                && radius != InvalidFloatValue
                // && expirationDuration != InvalidLongValue
                //&& transitionType != InvalidIntValue
                )
                return new GeofenceCircularRegion()
                {
                    Id = id,
                    Latitude = lat,
                    Longitude = lng,
                    Radius = radius,
                    NotifyOnEntry=notifyOnEntry,
                    NotifyOnExit=notifyOnExit
                };

			// Otherwise return null
			return null;
		}

		/// <summary>
		/// Save a geofence
		/// </summary>
		/// <param name="id">The ID of the Geofence</param>
		/// <param name="geofence">The SimpleGeofence with the values you want to save in SharedPreferemces</param>
		public override void SetGeofenceRegion(GeofenceCircularRegion region) {

            string id = region.Id;
			// Get a SharedPreferences editor instance. Among other things, SharedPreferences ensures that updates are atomic and non-concurrent
			ISharedPreferencesEditor prefs = mPrefs.Edit();
			// Write the geofence values to SharedPreferences 
            prefs.PutFloat(GetGeofenceFieldKey(id, LatitudeGeofenceRegionKey), (float)region.Latitude);
            prefs.PutFloat(GetGeofenceFieldKey(id, LongitudeGeofenceRegionKey), (float)region.Longitude);
            prefs.PutFloat(GetGeofenceFieldKey(id, RadiusGeofenceRegionKey), (float)region.Radius);
            prefs.PutBoolean(GetGeofenceFieldKey(id, NotifyOnEntryGeofenceRegionKey), region.NotifyOnEntry);
            prefs.PutBoolean(GetGeofenceFieldKey(id, NotifyOnExitGeofenceRegionKey), region.NotifyOnExit);
			// Commit the changes
			prefs.Commit ();
           
		}

        public override void ClearGeofenceRegions()
        {
            ISharedPreferencesEditor prefs = mPrefs.Edit();
            prefs.Clear();
            // Commit the changes
            prefs.Commit();
        }

        public override void RemoveGeofenceRegion(string id)
        {
            try
            {
                ISharedPreferencesEditor prefs = mPrefs.Edit();
         
                prefs.Remove(GetGeofenceFieldKey(id, LatitudeGeofenceRegionKey));
                prefs.Remove(GetGeofenceFieldKey(id, LongitudeGeofenceRegionKey));
                prefs.Remove(GetGeofenceFieldKey(id, RadiusGeofenceRegionKey));
                prefs.Remove(GetGeofenceFieldKey(id, NotifyOnEntryGeofenceRegionKey));
                prefs.Remove(GetGeofenceFieldKey(id, NotifyOnExitGeofenceRegionKey));
                // Commit the changes
                prefs.Commit();

            }catch(Java.Lang.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Error: {1}", CrossGeofence.Id, ex.ToString()));
            }
            
        }

        public override Dictionary<string,GeofenceCircularRegion> GetGeofenceRegions()
        {
            IEnumerable<string> keys = mPrefs.All.Where(p => p.Key.Split('_').Length>1).Select( p => p.Key.Split('_')[1]).Distinct();

            Dictionary<string, GeofenceCircularRegion> regions = new Dictionary<string,GeofenceCircularRegion>();

            foreach (string key in keys)
            {
        
              
                var region = GetGeofenceRegion(key);
                if (region != null)
                {
                   
                    regions.Add(region.Id, region);
                }
           
                
            }
           
            return regions;
            
        }

		
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if __UNIFIED__
using UIKit;
using Foundation;
#else
  using MonoTouch.UIKit;
  using MonoTouch.Foundation;
#endif
using Geofence.Plugin.Abstractions;
using System.Diagnostics;

namespace Geofence.Plugin
{
    internal class GeofenceStore : BaseGeofenceStore
    {
        private static GeofenceStore sharedInstance = new GeofenceStore();
        private string GeofenceIdsKey="Geofence.KeyIds";
        public static GeofenceStore SharedInstance { get { return sharedInstance; } }
        private ISet<string> geofenceIds;
        private const string IdSeparator = "|";
        private GeofenceStore()
        {
            geofenceIds = new HashSet<string>();

            if (!string.IsNullOrEmpty(NSUserDefaults.StandardUserDefaults.StringForKey(GeofenceIdsKey)))
            {
                //GeofenceIdsKey
                string[] keys = NSUserDefaults.StandardUserDefaults.StringForKey(GeofenceIdsKey).Split(IdSeparator[0]);

                foreach(string k in keys)
                {

                    geofenceIds.Add(k);

                }

            }

        }
       
        public override Dictionary<string, GeofenceCircularRegion> GetAll()
        {

            Dictionary<string, GeofenceCircularRegion> regions = new Dictionary<string, GeofenceCircularRegion>();

            foreach (NSString key in geofenceIds)
            {


                var region = Get(key.ToString());
                if (region != null)
                {
                  
                    regions.Add(region.Id, region);
                }


            }

            return regions;
        }

        public override GeofenceCircularRegion Get(string id)
        {
           GeofenceCircularRegion region = null;

           if(!string.IsNullOrEmpty(NSUserDefaults.StandardUserDefaults.StringForKey(GetFieldKey(id, IdGeofenceRegionKey))))
           {
               double lat = NSUserDefaults.StandardUserDefaults.DoubleForKey(GetFieldKey(id, LatitudeGeofenceRegionKey));
               double lon = NSUserDefaults.StandardUserDefaults.DoubleForKey(GetFieldKey(id, LongitudeGeofenceRegionKey));
               bool notifyOnEntry = NSUserDefaults.StandardUserDefaults.BoolForKey(GetFieldKey(id, NotifyOnEntryGeofenceRegionKey));
               bool notifyOnExit = NSUserDefaults.StandardUserDefaults.BoolForKey(GetFieldKey(id, NotifyOnExitGeofenceRegionKey));
               bool notifyOnStay = NSUserDefaults.StandardUserDefaults.BoolForKey(GetFieldKey(id, NotifyOnStayGeofenceRegionKey));
               double radius = NSUserDefaults.StandardUserDefaults.DoubleForKey(GetFieldKey(id, RadiusGeofenceRegionKey));
               string notificationEntryMessage = NSUserDefaults.StandardUserDefaults.StringForKey(GetFieldKey(id, NotificationEntryMessageGeofenceRegionKey));
               string notificationExitMessage = NSUserDefaults.StandardUserDefaults.StringForKey(GetFieldKey(id, NotificationExitMessageGeofenceRegionKey));
               string notificationStayMessage = NSUserDefaults.StandardUserDefaults.StringForKey(GetFieldKey(id, NotificationStayMessageGeofenceRegionKey));
               bool showNotification = NSUserDefaults.StandardUserDefaults.BoolForKey(GetFieldKey(id, ShowNotificationGeofenceRegionKey));
               bool persistent = NSUserDefaults.StandardUserDefaults.BoolForKey(GetFieldKey(id, PersistentGeofenceRegionKey));
               bool showEntryNotification = NSUserDefaults.StandardUserDefaults.BoolForKey(GetFieldKey(id, ShowEntryNotificationGeofenceRegionKey));
               bool showExitNotification = NSUserDefaults.StandardUserDefaults.BoolForKey(GetFieldKey(id, ShowExitNotificationGeofenceRegionKey));
               bool showStayNotification = NSUserDefaults.StandardUserDefaults.BoolForKey(GetFieldKey(id, ShowStayNotificationGeofenceRegionKey));
               int stayedInThresholdDuration = (int)NSUserDefaults.StandardUserDefaults.IntForKey(GetFieldKey(id, StayedInThresholdDurationGeofenceRegionKey));

               region = new GeofenceCircularRegion(id,lat,lon,radius,notifyOnEntry,notifyOnExit,notifyOnStay,showNotification,persistent,showEntryNotification,showExitNotification,showStayNotification)
               {
                   NotificationEntryMessage = notificationEntryMessage,
                   NotificationStayMessage = notificationStayMessage,
                   NotificationExitMessage = notificationExitMessage,
                   StayedInThresholdDuration=TimeSpan.FromMilliseconds(stayedInThresholdDuration)
               };
           }

           

           return region;
        }

        public override void Save(GeofenceCircularRegion region)
        {
            string id = region.Id;

            if(string.IsNullOrEmpty(id)||!region.Persistent)
              return;

            NSUserDefaults.StandardUserDefaults.SetString(region.Id,GetFieldKey(id, IdGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetDouble(region.Latitude,GetFieldKey(id, LatitudeGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetDouble(region.Longitude,GetFieldKey(id, LongitudeGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.NotifyOnEntry,GetFieldKey(id, NotifyOnEntryGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.NotifyOnExit,GetFieldKey(id, NotifyOnExitGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.NotifyOnStay,GetFieldKey(id, NotifyOnStayGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetDouble(region.Radius,GetFieldKey(id, RadiusGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.ShowNotification, GetFieldKey(id, ShowNotificationGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.Persistent, GetFieldKey(id, PersistentGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.ShowEntryNotification, GetFieldKey(id, ShowEntryNotificationGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.ShowExitNotification, GetFieldKey(id, ShowExitNotificationGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.ShowStayNotification, GetFieldKey(id, ShowStayNotificationGeofenceRegionKey));

            NSUserDefaults.StandardUserDefaults.SetInt((int)region.StayedInThresholdDuration.TotalMilliseconds, GetFieldKey(id, StayedInThresholdDurationGeofenceRegionKey));

            if(!string.IsNullOrEmpty(region.NotificationEntryMessage))
            {
               NSUserDefaults.StandardUserDefaults.SetString(region.NotificationEntryMessage, GetFieldKey(id, NotificationEntryMessageGeofenceRegionKey));
            }

            if (!string.IsNullOrEmpty(region.NotificationExitMessage))
            {
               NSUserDefaults.StandardUserDefaults.SetString(region.NotificationExitMessage, GetFieldKey(id, NotificationExitMessageGeofenceRegionKey));
            }
         
            if(!string.IsNullOrEmpty(region.NotificationStayMessage))
            {
               NSUserDefaults.StandardUserDefaults.SetString(region.NotificationStayMessage, GetFieldKey(id, NotificationStayMessageGeofenceRegionKey));
            }

            geofenceIds.Add(id);
            
            NSUserDefaults.StandardUserDefaults.SetString(string.Join(IdSeparator,geofenceIds.ToArray<string>()),GeofenceIdsKey);
            
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        public override void RemoveAll()
        {
            
            foreach (string key in geofenceIds)
            {
                ClearKeysForId(key);
            }
            NSUserDefaults.StandardUserDefaults.RemoveObject(GeofenceIdsKey);
            
        }

        public void ClearKeysForId(string id)
        {
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, IdGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, LatitudeGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, LongitudeGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, NotifyOnEntryGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, NotifyOnExitGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, NotifyOnStayGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, RadiusGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, NotificationEntryMessageGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, NotificationExitMessageGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, NotificationStayMessageGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, ShowNotificationGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, PersistentGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, ShowEntryNotificationGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, ShowExitNotificationGeofenceRegionKey));           
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, ShowStayNotificationGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.RemoveObject(GetFieldKey(id, StayedInThresholdDurationGeofenceRegionKey));
        }

        public override void Remove(string id)
        {
             if(!string.IsNullOrEmpty(NSUserDefaults.StandardUserDefaults.StringForKey(GetFieldKey(id, IdGeofenceRegionKey))))
             {

                 ClearKeysForId(id);


                if(geofenceIds.Contains(id))
                {
                         geofenceIds.Remove(id);

                         NSUserDefaults.StandardUserDefaults.SetString(string.Join(IdSeparator, geofenceIds.ToArray<string>()), GeofenceIdsKey);
                }
    
                 

                 NSUserDefaults.StandardUserDefaults.Synchronize();
             }
            
        }
    }
}
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
        private NSString GeofenceIdsKey=new NSString(@"Geofence.KeyIds");
        public static GeofenceStore SharedInstance { get { return sharedInstance; } }
        private NSMutableDictionary geofenceIds;
        private GeofenceStore()
        {
            if (NSUserDefaults.StandardUserDefaults.ValueForKey(GeofenceIdsKey)==null)
            {
                NSUserDefaults.StandardUserDefaults.SetValueForKey(new NSMutableDictionary(),GeofenceIdsKey);
            }

            geofenceIds = (NSMutableDictionary)NSUserDefaults.StandardUserDefaults.ValueForKey(GeofenceIdsKey);
        }
       
        public override Dictionary<string, GeofenceCircularRegion> GetGeofenceRegions()
        {

            Dictionary<string, GeofenceCircularRegion> regions = new Dictionary<string, GeofenceCircularRegion>();

            foreach (NSString key in geofenceIds.Keys)
            {


                var region = GetGeofenceRegion(key.ToString());
                if (region != null)
                {
                  
                    regions.Add(region.Id, region);
                }


            }

            return regions;
        }

        public override GeofenceCircularRegion GetGeofenceRegion(string id)
        {
           GeofenceCircularRegion region = null;

           if(!string.IsNullOrEmpty(NSUserDefaults.StandardUserDefaults.StringForKey(GetGeofenceFieldKey(id, IdGeofenceRegionKey))))
           {
               double lat = NSUserDefaults.StandardUserDefaults.DoubleForKey(GetGeofenceFieldKey(id, LatitudeGeofenceRegionKey));
               double lon = NSUserDefaults.StandardUserDefaults.DoubleForKey(GetGeofenceFieldKey(id, LongitudeGeofenceRegionKey));
               bool notifyOnEntry = NSUserDefaults.StandardUserDefaults.BoolForKey(GetGeofenceFieldKey(id, NotifyOnEntryGeofenceRegionKey));
               bool notifyOnExit = NSUserDefaults.StandardUserDefaults.BoolForKey(GetGeofenceFieldKey(id, NotifyOnExitGeofenceRegionKey));
               bool notifyOnStay = NSUserDefaults.StandardUserDefaults.BoolForKey(GetGeofenceFieldKey(id, NotifyOnStayGeofenceRegionKey));
               double radius = NSUserDefaults.StandardUserDefaults.DoubleForKey(GetGeofenceFieldKey(id, RadiusGeofenceRegionKey));
               string entryMessage = NSUserDefaults.StandardUserDefaults.StringForKey(GetGeofenceFieldKey(id, EntryMessageGeofenceRegionKey));
               string exitMessage = NSUserDefaults.StandardUserDefaults.StringForKey(GetGeofenceFieldKey(id, ExitMessageGeofenceRegionKey));
               string stayMessage = NSUserDefaults.StandardUserDefaults.StringForKey(GetGeofenceFieldKey(id, StayMessageGeofenceRegionKey));
              
               region = new GeofenceCircularRegion()
               {
                   Id = id,
                   Latitude = lat,
                   Longitude = lon,
                   Radius = radius,
                   NotifyOnEntry = notifyOnEntry,
                   NotifyOnExit = notifyOnExit,
                   NotifyOnStay = notifyOnStay,
                   EntryMessage = entryMessage,
                   StayMessage = stayMessage,
                   ExitMessage = exitMessage,
               };
           }

           

           return region;
        }

        public override void SetGeofenceRegion(GeofenceCircularRegion region)
        {
            string id = region.Id;

            if(string.IsNullOrEmpty(id))
              return;

            NSUserDefaults.StandardUserDefaults.SetString(region.Id,GetGeofenceFieldKey(id, IdGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetDouble(region.Latitude,GetGeofenceFieldKey(id, LatitudeGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetDouble(region.Longitude,GetGeofenceFieldKey(id, LongitudeGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.NotifyOnEntry,GetGeofenceFieldKey(id, NotifyOnEntryGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.NotifyOnExit,GetGeofenceFieldKey(id, NotifyOnExitGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetBool(region.NotifyOnStay,GetGeofenceFieldKey(id, NotifyOnStayGeofenceRegionKey));
            NSUserDefaults.StandardUserDefaults.SetDouble(region.Radius,GetGeofenceFieldKey(id, RadiusGeofenceRegionKey));
           
            if(!string.IsNullOrEmpty(region.EntryMessage))
            {
               NSUserDefaults.StandardUserDefaults.SetString(region.EntryMessage, GetGeofenceFieldKey(id, EntryMessageGeofenceRegionKey));
            }

            if (!string.IsNullOrEmpty(region.ExitMessage))
            {
               NSUserDefaults.StandardUserDefaults.SetString(region.ExitMessage, GetGeofenceFieldKey(id, ExitMessageGeofenceRegionKey));
            }
         
            if(!string.IsNullOrEmpty(region.StayMessage))
            {
               NSUserDefaults.StandardUserDefaults.SetString(region.StayMessage, GetGeofenceFieldKey(id, StayMessageGeofenceRegionKey));
            }

            geofenceIds.Add(new NSString(id),new NSString(GetGeofenceFieldKey(id, IdGeofenceRegionKey)));
           
            NSUserDefaults.StandardUserDefaults.SetValueForKey(geofenceIds, GeofenceIdsKey);
  
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        public override void ClearGeofenceRegions()
        {
            NSDictionary geofenceIds=(NSDictionary)NSUserDefaults.StandardUserDefaults.ValueForKey(GeofenceIdsKey);
   
            foreach (NSString key in geofenceIds.Keys)
            {
                RemoveGeofenceRegion(key.ToString());
            }

            
        }

        public override void RemoveGeofenceRegion(string id)
        {
             if(!string.IsNullOrEmpty(NSUserDefaults.StandardUserDefaults.StringForKey(GetGeofenceFieldKey(id, IdGeofenceRegionKey))))
             {
                 NSUserDefaults.StandardUserDefaults.RemoveObject(GetGeofenceFieldKey(id, IdGeofenceRegionKey));
                 NSUserDefaults.StandardUserDefaults.RemoveObject(GetGeofenceFieldKey(id, LatitudeGeofenceRegionKey));
                 NSUserDefaults.StandardUserDefaults.RemoveObject(GetGeofenceFieldKey(id, LongitudeGeofenceRegionKey));
                 NSUserDefaults.StandardUserDefaults.RemoveObject(GetGeofenceFieldKey(id, NotifyOnEntryGeofenceRegionKey));
                 NSUserDefaults.StandardUserDefaults.RemoveObject(GetGeofenceFieldKey(id, NotifyOnExitGeofenceRegionKey));
                 NSUserDefaults.StandardUserDefaults.RemoveObject(GetGeofenceFieldKey(id, NotifyOnStayGeofenceRegionKey));
                 NSUserDefaults.StandardUserDefaults.RemoveObject(GetGeofenceFieldKey(id, RadiusGeofenceRegionKey));
                 NSUserDefaults.StandardUserDefaults.RemoveObject(GetGeofenceFieldKey(id, EntryMessageGeofenceRegionKey));
                 NSUserDefaults.StandardUserDefaults.RemoveObject(GetGeofenceFieldKey(id, ExitMessageGeofenceRegionKey));
                 NSUserDefaults.StandardUserDefaults.RemoveObject(GetGeofenceFieldKey(id, StayMessageGeofenceRegionKey));

                 if(geofenceIds.Count>1)
                 {
                     NSString key = new NSString(id);

                     if(geofenceIds.ContainsKey(key))
                     {
                         geofenceIds.Remove(key);
                     }

                     NSUserDefaults.StandardUserDefaults.SetValueForKey(geofenceIds,GeofenceIdsKey);

                 }
                 else
                 {
                     NSUserDefaults.StandardUserDefaults.RemoveObject(GeofenceIdsKey);
                 }
                 

                 NSUserDefaults.StandardUserDefaults.Synchronize();
             }
            
        }
    }
}
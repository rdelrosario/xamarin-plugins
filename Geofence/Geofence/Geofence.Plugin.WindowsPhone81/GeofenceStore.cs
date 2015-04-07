using Geofence.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Geofence.Plugin
{
    internal class GeofenceStore : BaseGeofenceStore
    {
        private static GeofenceStore sharedInstance = new GeofenceStore();

        public static GeofenceStore SharedInstance { get { return sharedInstance; } }

        public static ApplicationDataContainer Container
        {
            get
            {
                if (!ApplicationData.Current.LocalSettings.Containers.Any(x => x.Key == GeofenceStoreId))
                {
                    ApplicationData.Current.LocalSettings.CreateContainer(GeofenceStoreId, Windows.Storage.ApplicationDataCreateDisposition.Always);
                }
                return ApplicationData.Current.LocalSettings.Containers[GeofenceStoreId];
            }
        }


        public override Dictionary<string, GeofenceCircularRegion> GetGeofenceRegions()
        {
            IEnumerable<string> keys = Container.Values.Where(p => p.Key.Split('_').Length > 1).Select(p => p.Key.Split('_')[1]).Distinct();

            Dictionary<string, GeofenceCircularRegion> regions = new Dictionary<string, GeofenceCircularRegion>();

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

        public override GeofenceCircularRegion GetGeofenceRegion(string id)
        {
            GeofenceCircularRegion region=null;
            if (Container.Values.ContainsKey(GetGeofenceFieldKey(id, IdGeofenceRegionKey))
                && Container.Values.ContainsKey(GetGeofenceFieldKey(id, LatitudeGeofenceRegionKey))
                && Container.Values.ContainsKey(GetGeofenceFieldKey(id, LongitudeGeofenceRegionKey))
                && Container.Values.ContainsKey(GetGeofenceFieldKey(id, NotifyOnEntryGeofenceRegionKey))
                && Container.Values.ContainsKey(GetGeofenceFieldKey(id, NotifyOnExitGeofenceRegionKey))
                && Container.Values.ContainsKey(GetGeofenceFieldKey(id, RadiusGeofenceRegionKey)))
            {
                region= new GeofenceCircularRegion()
                {
                    Id = Container.Values[GetGeofenceFieldKey(id, IdGeofenceRegionKey)].ToString(),
                    Latitude = (double)Container.Values[GetGeofenceFieldKey(id, LatitudeGeofenceRegionKey)],
                    Longitude = (double)Container.Values[GetGeofenceFieldKey(id, LongitudeGeofenceRegionKey)],
                    Radius = (double)Container.Values[GetGeofenceFieldKey(id, RadiusGeofenceRegionKey)],
                    NotifyOnEntry = (bool)Container.Values[GetGeofenceFieldKey(id, NotifyOnEntryGeofenceRegionKey)],
                    NotifyOnExit = (bool)Container.Values[GetGeofenceFieldKey(id, NotifyOnExitGeofenceRegionKey)],
                };
            }

            return region;
        }

        public override void SetGeofenceRegion(GeofenceCircularRegion region)
        {
            string id = region.Id;

            Container.Values[GetGeofenceFieldKey(id, IdGeofenceRegionKey)] = region.Id;
            Container.Values[GetGeofenceFieldKey(id, LatitudeGeofenceRegionKey)] = region.Latitude;
            Container.Values[GetGeofenceFieldKey(id, LongitudeGeofenceRegionKey)] = region.Longitude;
            Container.Values[GetGeofenceFieldKey(id, NotifyOnEntryGeofenceRegionKey)] = region.NotifyOnEntry;
            Container.Values[GetGeofenceFieldKey(id, NotifyOnExitGeofenceRegionKey)] = region.NotifyOnExit;
            Container.Values[GetGeofenceFieldKey(id, RadiusGeofenceRegionKey)] = region.Radius;
            
        }

        public override void ClearGeofenceRegions()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(GeofenceStoreId);
        }

        public override void RemoveGeofenceRegion(string id)
        {
            Container.Values.Remove(GetGeofenceFieldKey(id, IdGeofenceRegionKey));
            Container.Values.Remove(GetGeofenceFieldKey(id, LatitudeGeofenceRegionKey));
            Container.Values.Remove(GetGeofenceFieldKey(id, LongitudeGeofenceRegionKey));
            Container.Values.Remove(GetGeofenceFieldKey(id, NotifyOnEntryGeofenceRegionKey));
            Container.Values.Remove(GetGeofenceFieldKey(id, NotifyOnExitGeofenceRegionKey));
            Container.Values.Remove(GetGeofenceFieldKey(id, RadiusGeofenceRegionKey));
        }
    }
}

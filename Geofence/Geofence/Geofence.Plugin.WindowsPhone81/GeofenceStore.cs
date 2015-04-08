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


        public override Dictionary<string, GeofenceCircularRegion> GetAll()
        {
            IEnumerable<string> keys = Container.Values.Where(p => p.Key.ToString().StartsWith(GeofenceStoreId) && p.Key.Split('_').Length > 1).Select(p => p.Key.Split('_')[1]).Distinct();

            Dictionary<string, GeofenceCircularRegion> regions = new Dictionary<string, GeofenceCircularRegion>();

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

        public override GeofenceCircularRegion Get(string id)
        {
            GeofenceCircularRegion region=null;
            if (Container.Values.ContainsKey(GetFieldKey(id, IdGeofenceRegionKey))
                && Container.Values.ContainsKey(GetFieldKey(id, LatitudeGeofenceRegionKey))
                && Container.Values.ContainsKey(GetFieldKey(id, LongitudeGeofenceRegionKey))
                && Container.Values.ContainsKey(GetFieldKey(id, NotifyOnEntryGeofenceRegionKey))
                && Container.Values.ContainsKey(GetFieldKey(id, NotifyOnExitGeofenceRegionKey))
                && Container.Values.ContainsKey(GetFieldKey(id, RadiusGeofenceRegionKey)))
            {
                region= new GeofenceCircularRegion()
                {
                    Id = Container.Values[GetFieldKey(id, IdGeofenceRegionKey)].ToString(),
                    Latitude = (double)Container.Values[GetFieldKey(id, LatitudeGeofenceRegionKey)],
                    Longitude = (double)Container.Values[GetFieldKey(id, LongitudeGeofenceRegionKey)],
                    Radius = (double)Container.Values[GetFieldKey(id, RadiusGeofenceRegionKey)],
                    NotifyOnEntry = (bool)Container.Values[GetFieldKey(id, NotifyOnEntryGeofenceRegionKey)],
                    NotifyOnExit = (bool)Container.Values[GetFieldKey(id, NotifyOnExitGeofenceRegionKey)],
                };
            }

            return region;
        }

        public override void Save(GeofenceCircularRegion region)
        {
            string id = region.Id;

            Container.Values[GetFieldKey(id, IdGeofenceRegionKey)] = region.Id;
            Container.Values[GetFieldKey(id, LatitudeGeofenceRegionKey)] = region.Latitude;
            Container.Values[GetFieldKey(id, LongitudeGeofenceRegionKey)] = region.Longitude;
            Container.Values[GetFieldKey(id, NotifyOnEntryGeofenceRegionKey)] = region.NotifyOnEntry;
            Container.Values[GetFieldKey(id, NotifyOnExitGeofenceRegionKey)] = region.NotifyOnExit;
            Container.Values[GetFieldKey(id, RadiusGeofenceRegionKey)] = region.Radius;
            
        }

        public override void RemoveAll()
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(GeofenceStoreId);
        }

        public override void Remove(string id)
        {
            Container.Values.Remove(GetFieldKey(id, IdGeofenceRegionKey));
            Container.Values.Remove(GetFieldKey(id, LatitudeGeofenceRegionKey));
            Container.Values.Remove(GetFieldKey(id, LongitudeGeofenceRegionKey));
            Container.Values.Remove(GetFieldKey(id, NotifyOnEntryGeofenceRegionKey));
            Container.Values.Remove(GetFieldKey(id, NotifyOnExitGeofenceRegionKey));
            Container.Values.Remove(GetFieldKey(id, RadiusGeofenceRegionKey));
        }
    }
}

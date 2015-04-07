using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    interface IGeofenceStore
    {
        Dictionary<string, GeofenceCircularRegion> GetGeofenceRegions();
        GeofenceCircularRegion GetGeofenceRegion(string id);
        void SetGeofenceRegion(GeofenceCircularRegion region);
        void ClearGeofenceRegions();
        void RemoveGeofenceRegion(String id);
    }
}

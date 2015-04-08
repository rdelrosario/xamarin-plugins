using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    interface IGeofenceStore
    {
        Dictionary<string, GeofenceCircularRegion> GetAll();
        GeofenceCircularRegion Get(string id);
        void Save(GeofenceCircularRegion region);
        void RemoveAll();
        void Remove(String id);
    }
}

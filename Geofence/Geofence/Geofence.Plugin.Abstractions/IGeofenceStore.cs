using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    /// <summary>
    /// Geofence regions store interface
    /// </summary>
    interface IGeofenceStore
    {
        /// <summary>
        /// Gets all geofence regions in store
        /// </summary>
        /// <returns></returns>
        Dictionary<string, GeofenceCircularRegion> GetAll();
        /// <summary>
        /// Gets an specific geofence from store
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        GeofenceCircularRegion Get(string id);
        /// <summary>
        /// Save geofence region in store
        /// </summary>
        /// <param name="region"></param>
        void Save(GeofenceCircularRegion region);
        /// <summary>
        /// Remove all geofences regions from store
        /// </summary>
        void RemoveAll();
        /// <summary>
        /// Remove specific geofence region from store
        /// </summary>
        /// <param name="id"></param>
        void Remove(String id);
    }
}

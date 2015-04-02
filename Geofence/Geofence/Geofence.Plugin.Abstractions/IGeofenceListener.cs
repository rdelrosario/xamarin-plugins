using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    public interface IGeofenceListener
    {
       void OnMonitoringStarted(GeofenceCircularRegion region);
       void OnMonitoringStopped();
       void OnRegionEntered(GeofenceResult result);
       void OnRegionExited(GeofenceResult result);
       void OnRegionStay(GeofenceResult result);
       void OnError(String error);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    public interface IGeofenceListener
    {
       void OnMonitoringStarted(GeoCircularRegion region);
       void OnMonitoringStopped();
       void OnRegionEntered(GeoCircularRegion region);
       void OnRegionExited(GeoCircularRegion region);
       void OnError(String error);
    }
}

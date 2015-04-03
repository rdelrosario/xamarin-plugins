using System;
using System.Collections.Generic;

namespace Geofence.Plugin.Abstractions
{
  /// <summary>
  /// Interface for Geofence
  /// </summary>
  public interface IGeofence
  {
      IList<GeofenceCircularRegion> Regions {get; }
      bool IsMonitoring { get; }
      void StartMonitoring(List<GeofenceCircularRegion> regions);
      void StopMonitoring();
  }
}

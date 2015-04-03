using System;
using System.Collections.Generic;

namespace Geofence.Plugin.Abstractions
{
  /// <summary>
  /// Interface for Geofence
  /// </summary>
  public interface IGeofence
  {
      IEnumerable<GeofenceCircularRegion> Regions {get; }
      bool IsMonitoring { get; }
      void StartMonitoring(IList<GeofenceCircularRegion> regions);
      void StopMonitoring();
      void StopMonitoring(string identifier);
      void StopMonitoring(List<string> identifier);

  }
}

using System;
using System.Collections.Generic;

namespace Geofence.Plugin.Abstractions
{
  /// <summary>
  /// Interface for Geofence
  /// </summary>
  public interface IGeofence
  {
      IReadOnlyDictionary<string, GeofenceCircularRegion> Regions { get; }
      IReadOnlyDictionary<string, GeofenceResult> GeofenceResults { get; }
      bool IsMonitoring { get; }
      void StartMonitoring(IList<GeofenceCircularRegion> regions);

      //void StartMonitoring(GeofenceCircularRegion region);
      void StopMonitoring();
      void StopMonitoring(string identifier);
      void StopMonitoring(List<string> identifier);

  }
}

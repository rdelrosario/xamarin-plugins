using System;
using System.Collections.Generic;

namespace Geofence.Plugin.Abstractions
{
  /// <summary>
  /// Interface for Geofence
  /// </summary>
  public interface IGeofence
  {
     // List<GeoCircularRegion> Regions {get; }
      void Start(List<GeoCircularRegion> regions);
      void Stop();
  }
}

using System;
using System.Collections.Generic;

namespace Geofence.Plugin.Abstractions
{
  /// <summary>
  /// Interface for Geofence
  /// </summary>
  public interface IGeofence
  {
      /// <summary>
      /// Dictionary that contains all regions been monitored
      /// </summary>
      IReadOnlyDictionary<string, GeofenceCircularRegion> Regions { get; }
      /// <summary>
      /// Dicitonary that contains all geofence results received
      /// </summary>
      IReadOnlyDictionary<string, GeofenceResult> GeofenceResults { get; }
      /// <summary>
      /// Indicator that is true if at least one region is been monitored
      /// </summary>
      bool IsMonitoring { get; }
      /// <summary>
      /// Last known geofence location
      /// </summary>
      GeofenceLocation LastKnownLocation { get; }
      /// <summary>
      /// Starts monitoring one region
      /// </summary>
      /// <param name="regions"></param>
      void StartMonitoring(GeofenceCircularRegion region);
      /// <summary>
      /// Starts monitoring multiple regions
      /// </summary>
      /// <param name="regions"></param>
      void StartMonitoring(IList<GeofenceCircularRegion> regions);
      /// <summary>
      /// Stops monitoring one region
      /// </summary>
      /// <param name="identifier"></param>
      void StopMonitoring(string identifier);
      /// <summary>
      /// Stops monitoring multiple regions
      /// </summary>
      /// <param name="identifier"></param>
      void StopMonitoring(IList<string> identifier);
      /// <summary>
      /// Stops monitoring all regions
      /// </summary>
      void StopMonitoringAllRegions();
      /// <summary>
      /// Determines whether location enabled and returns the result to the specified action.
      /// </summary>
      /// <returns><c>true</c> if this instance is location enabled the specified returnAction; otherwise, <c>false</c>.</returns>
      /// <param name="returnAction">Return action.</param>
      void IsLocationEnabled(Action<bool> returnAction);



  }
}

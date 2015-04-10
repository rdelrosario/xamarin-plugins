using Geofence.Plugin.Abstractions;
using System;


namespace Geofence.Plugin
{
  /// <summary>
  /// Implementation for Geofence
  /// </summary>
  public class GeofenceImplementation : IGeofence
  {

      public System.Collections.Generic.IReadOnlyDictionary<string, GeofenceCircularRegion> Regions
      {
          get { throw new NotImplementedException(); }
      }

      public System.Collections.Generic.IReadOnlyDictionary<string, GeofenceResult> GeofenceResults
      {
          get { throw new NotImplementedException(); }
      }

      public bool IsMonitoring
      {
          get { throw new NotImplementedException(); }
      }

      public void StartMonitoring(System.Collections.Generic.IList<GeofenceCircularRegion> regions)
      {
          throw new NotImplementedException();
      }

      public void StopMonitoring()
      {
          throw new NotImplementedException();
      }

      public void StopMonitoring(string identifier)
      {
          throw new NotImplementedException();
      }

      public void StopMonitoring(System.Collections.Generic.List<string> identifier)
      {
          throw new NotImplementedException();
      }
  }
}
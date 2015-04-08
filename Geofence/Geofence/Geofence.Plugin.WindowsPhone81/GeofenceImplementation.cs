using Geofence.Plugin.Abstractions;
using System;
using Windows.Devices.Geolocation;
using System.Linq;

namespace Geofence.Plugin
{
  /// <summary>
  /// Implementation for Geofence
  /// </summary>
  public class GeofenceImplementation : IGeofence
  {
      public GeofenceImplementation()
      {
          Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.GeofenceStateChanged += GeofenceStateChanged;
      }

      void GeofenceStateChanged(Windows.Devices.Geolocation.Geofencing.GeofenceMonitor sender, object args)
      {
          //sender.Geofences
      }
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
          get { return Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.Geofences.Count>0; }
      }

      public void StartMonitoring(System.Collections.Generic.IList<GeofenceCircularRegion> regions)
      {
          foreach(GeofenceCircularRegion region in regions)
          {
              AddRegion(region);
             
          }
      }
      public void RemoveRegion(string id)
      {
          var geofence = Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.Geofences.FirstOrDefault(x => x.Id == id);
          if (geofence != null)
          {
              Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.Geofences.Remove(geofence);
          }
          
          GeofenceStore.SharedInstance.Remove(id);
      }

      public void AddRegion(GeofenceCircularRegion region)
      {
          if (string.IsNullOrEmpty(region.Id))
              return;

          RemoveRegion(region.Id);

          var position = new BasicGeoposition();
          position.Latitude = region.Latitude;
          position.Longitude = region.Longitude;

          var geocircle = new Geocircle(position, region.Radius);

          Windows.Devices.Geolocation.Geofencing.MonitoredGeofenceStates mask = 0;

          if (region.NotifyOnEntry)
          {
              mask |= Windows.Devices.Geolocation.Geofencing.MonitoredGeofenceStates.Entered;
          }

          if (region.NotifyOnExit)
          {
              mask |= Windows.Devices.Geolocation.Geofencing.MonitoredGeofenceStates.Exited;
          }


          var geofence = new Windows.Devices.Geolocation.Geofencing.Geofence(region.Id, geocircle, mask, false, new TimeSpan(0, 0, CrossGeofence.StayedInDuration / 1000), DateTime.Now, TimeSpan.MaxValue);
          Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.Geofences.Add(geofence);

          GeofenceStore.SharedInstance.Save(region);
      }
      public void StopMonitoring()
      {
         /* foreach (Windows.Devices.Geolocation.Geofencing.Geofence region in Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.Geofences)
          {
              StopMonitoring(region.Id); 
          }*/
          Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.Geofences.Clear();
          GeofenceStore.SharedInstance.RemoveAll();
      }

      public void StopMonitoring(string identifier)
      {
          var geofence = Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.Geofences.SingleOrDefault(g => g.Id == identifier);

          if (geofence != null)
              Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.Geofences.Remove(geofence);         

      }

      public void StopMonitoring(System.Collections.Generic.List<string> identifiers)
      {
         foreach(var identifier in identifiers)
         {
             StopMonitoring(identifier); 
         }
      
      }
  }
}
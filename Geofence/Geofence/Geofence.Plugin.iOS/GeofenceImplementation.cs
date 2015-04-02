using Geofence.Plugin.Abstractions;
using MonoTouch.CoreLocation;
using System;
using System.Collections.Generic;


namespace Geofence.Plugin
{
  /// <summary>
  /// Implementation for Geofence
  /// </summary>
  public class GeofenceImplementation : IGeofence
  {
      CLLocationManager locationManager;
      private List<CLCircularRegion> mGeofenceList;
      public IGeofenceListener Listener { get; set; }

      public GeofenceImplementation()
      {
          locationManager = new CLLocationManager();
          locationManager.DidStartMonitoringForRegion += DidStartMonitoringForRegion;
          locationManager.RegionEntered += RegionEntered;
          locationManager.RegionLeft +=RegionLeft;
          locationManager.Failed += OnFailure;
      }

      void OnFailure(object sender, MonoTouch.Foundation.NSErrorEventArgs e)
      {
          Listener.OnError(e.Error.LocalizedDescription);
      }

      void RegionEntered(object sender, CLRegionEventArgs e)
      {
          Listener.OnRegionEntered(new GeoCircularRegion()
          {
              Latitude=e.Region.Center.Latitude,
              Longitude = e.Region.Center.Longitude,
              Tag = e.Region.Identifier
          });
      }

      void RegionLeft(object sender, CLRegionEventArgs e)
      {
          Listener.OnRegionExited(new GeoCircularRegion()
          {
              Latitude = e.Region.Center.Latitude,
              Longitude = e.Region.Center.Longitude,
              Tag = e.Region.Identifier
          });
      }

      void DidStartMonitoringForRegion(object sender, CLRegionEventArgs e)
      {
          Listener.OnMonitoringStarted(new GeoCircularRegion()
          {
              Latitude = e.Region.Center.Latitude,
              Longitude = e.Region.Center.Longitude,
              Tag = e.Region.Identifier
          });
      }

      public void Start(List<GeoCircularRegion> regions)
      {
          mGeofenceList.Clear();
          if (CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion)))
          {

              foreach (GeoCircularRegion region in regions)
              {
                  var cRegion = new CLCircularRegion(new CLLocationCoordinate2D(region.Latitude, region.Longitude), region.Radius, region.Tag);
                    mGeofenceList.Add(cRegion);

                  locationManager.StartMonitoring(cRegion);
              }

          }
          else
          {
              Console.WriteLine("This plugin requires region monitoring, which is unavailable on this device");
          }
          
      }

      public void Stop()
      {
          foreach (CLCircularRegion region in mGeofenceList)
          {
              if (CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion)))
              {
                  locationManager.StopMonitoring(region);
               
              }
          }
          Listener.OnMonitoringStopped();
         
      }
  }
}
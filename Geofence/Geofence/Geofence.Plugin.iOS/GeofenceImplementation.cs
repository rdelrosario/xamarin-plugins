using Geofence.Plugin.Abstractions;
#if __UNIFIED__
  using CoreLocation;
#else
  using MonoTouch.CoreLocation;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geofence.Plugin
{
  /// <summary>
  /// Implementation for Geofence
  /// </summary>
  public class GeofenceImplementation : IGeofence
  {
      CLLocationManager locationManager;
      private List<CLCircularRegion> mGeofenceList;
      private Dictionary<string, GeofenceCircularRegion> mRegions;
      private Dictionary<string, GeofenceResult> mGeoreferenceResults;
      public List<GeofenceCircularRegion> Regions { get { return mRegions.Values.ToList(); } }
      public bool IsMonitoring { get { return mRegions.Count > 0; } }

      public GeofenceImplementation()
      {
          mGeofenceList = new List<CLCircularRegion>();
          mRegions = new Dictionary<string, GeofenceCircularRegion>();
          mGeoreferenceResults = new Dictionary<string, GeofenceResult>();

          locationManager = new CLLocationManager();
          locationManager.DidStartMonitoringForRegion += DidStartMonitoringForRegion;
          locationManager.RegionEntered += RegionEntered;
          locationManager.RegionLeft +=RegionLeft;
          locationManager.Failed += OnFailure;
       

      }

      void OnFailure(object sender, MonoTouch.Foundation.NSErrorEventArgs e)
      {
          if (IsMonitoring)
          {
              StopMonitoring();
          }
          CrossGeofence.GeofenceListener.OnError(e.Error.LocalizedDescription);
      }

      async void RegionEntered(object sender, CLRegionEventArgs e)
      {
          if (!mGeoreferenceResults.ContainsKey(e.Region.Identifier))
          {
              mGeoreferenceResults.Add(e.Region.Identifier, new GeofenceResult()
              {
                  Region = mRegions[e.Region.Identifier]
              });
          }

          mGeoreferenceResults[e.Region.Identifier].LastEnterTime = DateTime.UtcNow;
          mGeoreferenceResults[e.Region.Identifier].LastExitTime = null;

          CrossGeofence.GeofenceListener.OnRegionEntered(mGeoreferenceResults[e.Region.Identifier]);

          if (mGeoreferenceResults[e.Region.Identifier].Region.MinimumDuration != 0)
          {
              await Task.Delay(mGeoreferenceResults[e.Region.Identifier].Region.MinimumDuration);

              if (mGeoreferenceResults[e.Region.Identifier].LastExitTime == null)
              {
                  CrossGeofence.GeofenceListener.OnRegionStay(mGeoreferenceResults[e.Region.Identifier]);
              }
          }
      }

      void RegionLeft(object sender, CLRegionEventArgs e)
      {
          if (!mGeoreferenceResults.ContainsKey(e.Region.Identifier))
          {
              mGeoreferenceResults.Add(e.Region.Identifier, new GeofenceResult()
              {
                  Region = mRegions[e.Region.Identifier]
              });
          }

          mGeoreferenceResults[e.Region.Identifier].LastExitTime = DateTime.UtcNow;

          CrossGeofence.GeofenceListener.OnRegionExited(mGeoreferenceResults[e.Region.Identifier]);
      }

      void DidStartMonitoringForRegion(object sender, CLRegionEventArgs e)
      {
          CrossGeofence.GeofenceListener.OnMonitoringStarted(mRegions[e.Region.Identifier]);
      }

      public void StartMonitoring(List<GeofenceCircularRegion> regions)
      {
          if(IsMonitoring)
          {
              StopMonitoring();
          }
        
          if (CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion)))
          {

              foreach (GeofenceCircularRegion region in regions)
              {
                  var cRegion = new CLCircularRegion(new CLLocationCoordinate2D(region.Latitude, region.Longitude), region.Radius, region.Tag);
                  mGeofenceList.Add(cRegion);
                  mRegions.Add(region.Tag, region);
                  locationManager.StartMonitoring(cRegion);
              }

          }
          else
          {
              Console.WriteLine("This plugin requires region monitoring, which is unavailable on this device");
          }
          
      }

      public void StopMonitoring()
      {
          foreach (CLCircularRegion region in mGeofenceList)
          {
              if (CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion)))
              {
                  locationManager.StopMonitoring(region);
               
              }
          }
          mGeofenceList.Clear();
          mRegions.Clear();
          mGeoreferenceResults.Clear();
          CrossGeofence.GeofenceListener.OnMonitoringStopped();
         
      }
  }
}
using Geofence.Plugin.Abstractions;
#if __UNIFIED__
  using CoreLocation;
  using UIKit;
  using Foundation;
#else
  using MonoTouch.CoreLocation;
  using MonoTouch.UIKit;
  using MonoTouch.Foundation;
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
      private List<CLRegion> mGeofenceList;
      private Dictionary<string, GeofenceCircularRegion> mRegions;
      private Dictionary<string, GeofenceResult> mGeoreferenceResults;
      public IEnumerable<GeofenceCircularRegion> Regions { get { return mRegions.Values; } }
      public bool IsMonitoring { get { return mRegions.Count > 0; } }

      public GeofenceImplementation()
      {
          mGeofenceList = new List<CLRegion>();
          mRegions = new Dictionary<string, GeofenceCircularRegion>();
          mGeoreferenceResults = new Dictionary<string, GeofenceResult>();

          locationManager = new CLLocationManager();
          locationManager.DidStartMonitoringForRegion += DidStartMonitoringForRegion;
          locationManager.RegionEntered += RegionEntered;
          locationManager.RegionLeft +=RegionLeft;
          locationManager.Failed += OnFailure;
       

      }

      void OnFailure(object sender, NSErrorEventArgs e)
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
                  RegionId = e.Region.Identifier
              });
          }
          mGeoreferenceResults[e.Region.Identifier].Latitude = e.Region.Center.Latitude;
          mGeoreferenceResults[e.Region.Identifier].Latitude = e.Region.Center.Longitude;
          mGeoreferenceResults[e.Region.Identifier].LastEnterTime = DateTime.UtcNow;
          mGeoreferenceResults[e.Region.Identifier].LastExitTime = null;
          mGeoreferenceResults[e.Region.Identifier].Transition = GeofenceTransition.Entered;
          CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeoreferenceResults[e.Region.Identifier]);

          if (mRegions.ContainsKey(e.Region.Identifier)&&mRegions[e.Region.Identifier].MinimumDuration != 0)
          {
              await Task.Delay(mRegions[e.Region.Identifier].MinimumDuration);

              if (mGeoreferenceResults[e.Region.Identifier].LastExitTime == null)
              {
                  mGeoreferenceResults[e.Region.Identifier].Transition = GeofenceTransition.Stayed;
                  CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeoreferenceResults[e.Region.Identifier]);
              }
          }
      }

      void RegionLeft(object sender, CLRegionEventArgs e)
      {
          if (!mGeoreferenceResults.ContainsKey(e.Region.Identifier))
          {
              mGeoreferenceResults.Add(e.Region.Identifier, new GeofenceResult()
              {
                  RegionId=e.Region.Identifier
              });
          }
          mGeoreferenceResults[e.Region.Identifier].Latitude = e.Region.Center.Latitude;
          mGeoreferenceResults[e.Region.Identifier].Latitude = e.Region.Center.Longitude;
          mGeoreferenceResults[e.Region.Identifier].LastExitTime = DateTime.UtcNow;
          mGeoreferenceResults[e.Region.Identifier].Transition = GeofenceTransition.Exited;
          CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeoreferenceResults[e.Region.Identifier]);
      }

      void DidStartMonitoringForRegion(object sender, CLRegionEventArgs e)
      {
          CrossGeofence.GeofenceListener.OnMonitoringStarted();
      }

      public void StartMonitoring(IList<GeofenceCircularRegion> regions)
      {
          if (!CrossGeofence.IsInitialized)
          {
              throw NewGeofenceNotInitializedException();
          }

          if (!CLLocationManager.LocationServicesEnabled)
          {
              string message = string.Format("{0} - {1}", CrossGeofence.Tag, "You need to enable Location Services");
              System.Diagnostics.Debug.WriteLine(message);
              CrossGeofence.GeofenceListener.OnError(message);
          }
          else if (CLLocationManager.Status == CLAuthorizationStatus.Denied || CLLocationManager.Status == CLAuthorizationStatus.Restricted)
          {
              string message = string.Format("{0} - {1}", CrossGeofence.Tag, "You need to authorize Location Services");
              System.Diagnostics.Debug.WriteLine(message);
              CrossGeofence.GeofenceListener.OnError(message);

          }else if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
          {
              if (IsMonitoring)
              {
                  StopMonitoring();
              }

              foreach (GeofenceCircularRegion region in regions)
              {
                  CLRegion cRegion=null;

                  if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                  {
                      cRegion = new CLCircularRegion(new CLLocationCoordinate2D(region.Latitude, region.Longitude), (region.Radius > locationManager.MaximumRegionMonitoringDistance)?locationManager.MaximumRegionMonitoringDistance:region.Radius, region.Tag);
                  }
                  else
                  {
                      cRegion = new CLRegion(new CLLocationCoordinate2D(region.Latitude, region.Longitude), (region.Radius > locationManager.MaximumRegionMonitoringDistance) ? locationManager.MaximumRegionMonitoringDistance : region.Radius, region.Tag);
                  }

                  mGeofenceList.Add(cRegion);
                  mRegions.Add(region.Tag, region);
                  locationManager.StartMonitoring(cRegion);
              }

          }
          else
          {
              string message = string.Format("{0} - {1}",CrossGeofence.Tag,"Plugin requires region monitoring, which is unavailable on this device");
              System.Diagnostics.Debug.WriteLine(message);

              CrossGeofence.GeofenceListener.OnError(message);
          }
          
      }

      public void StopMonitoring()
      {
          if (!CrossGeofence.IsInitialized)
          {
              throw NewGeofenceNotInitializedException();
          }

          foreach (CLCircularRegion region in mGeofenceList)
          {
              if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
              {
                  locationManager.StopMonitoring(region);
               
              }
          }
          mGeofenceList.Clear();
          mRegions.Clear();
          mGeoreferenceResults.Clear();

   
          CrossGeofence.GeofenceListener.OnMonitoringStopped();
        
         
         
      }
      GeofenceNotInitializedException NewGeofenceNotInitializedException()
      {
          string description = string.Format("{0} - {1}", CrossGeofence.Tag, "Plugin is not initialized. Should initialize before use with CrossGeofence Initialize method. Example:  CrossGeofence.Initialize<CrossGeofenceListener>()");

          return new GeofenceNotInitializedException(description);
      }

      public void StopMonitoring(string regionIdentifier)
      {
          if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
          {
              var region = GetRegion(regionIdentifier);
              if (region != null)
              {
                  locationManager.StopMonitoring(region);
                  mGeofenceList.Remove(region);
                  RemoveRegion(regionIdentifier);

                  CrossGeofence.GeofenceListener.OnMonitoringStopped(regionIdentifier);
              }
              else
              {
                  System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Tag, "Region Identifier: " + regionIdentifier + " isn't being monitored"));
              }

              if (mRegions.Count == 0)
              {
                  CrossGeofence.GeofenceListener.OnMonitoringStopped();
              }
             
       
          }


      }

      public void StopMonitoring(List<string> regionIdentifiers)
      {
          if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
          {
              foreach(string regionIdentifier in regionIdentifiers)
              {
                  var region = GetRegion(regionIdentifier);
                  if (region != null)
                  {
                     locationManager.StopMonitoring(region);
                     mGeofenceList.Remove(region);
                     RemoveRegion(regionIdentifier);

                     CrossGeofence.GeofenceListener.OnMonitoringStopped(regionIdentifier);
                  }
                  else
                  {
                     System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Tag, "Region Identifier: " + regionIdentifier + " isn't being monitored"));
                  }

              }

              if (mRegions.Count == 0)
              {
                  CrossGeofence.GeofenceListener.OnMonitoringStopped();
              }
          }


      }

      private void RemoveRegion(string regionIdentifer)
      {
          if (mRegions.ContainsKey(regionIdentifer))
          {
              mRegions.Remove(regionIdentifer);
          }

          if (mGeoreferenceResults.ContainsKey(regionIdentifer))
          {
              mGeoreferenceResults.Remove(regionIdentifer);
          }
      }

      private CLRegion GetRegion(string identifier)
      {
          CLRegion region = null;
          IEnumerable<CLRegion> regionEnumerable = mGeofenceList.Where(s => s.Identifier.Equals(identifier, StringComparison.Ordinal));
          if (regionEnumerable != null)
          {
              region = regionEnumerable.First();
          }
          return region;
      }

      public void StartMonitoring(GeofenceCircularRegion region)
      {
          if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
          {
              CLRegion cRegion = null;

              if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
              {
                  cRegion = new CLCircularRegion(new CLLocationCoordinate2D(region.Latitude, region.Longitude), (region.Radius > locationManager.MaximumRegionMonitoringDistance) ? locationManager.MaximumRegionMonitoringDistance : region.Radius, region.Tag);
              }
              else
              {
                  cRegion = new CLRegion(new CLLocationCoordinate2D(region.Latitude, region.Longitude), (region.Radius > locationManager.MaximumRegionMonitoringDistance) ? locationManager.MaximumRegionMonitoringDistance : region.Radius, region.Tag);
              }
              if (!mRegions.ContainsKey(region.Tag))
              {
                  mRegions.Add(region.Tag, region);
                  mGeofenceList.Add(cRegion);
                  locationManager.StartMonitoring(cRegion);
                  //CrossGeofence.GeofenceListener.OnRegionMonitoringStarted(region);
              }
              else
              {
                  System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Tag, "Region: "+ region.Tag +" already been monitored"));
              }

             
          }

         
          

      }


  }
}
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
using System.Collections.ObjectModel;


namespace Geofence.Plugin
{
  /// <summary>
  /// Implementation for Geofence
  /// </summary>
  public class GeofenceImplementation : IGeofence
  {
     
      CLLocationManager locationManager;

      private Dictionary<string, GeofenceCircularRegion> mRegions=GeofenceStore.SharedInstance.GetAll();
      private Dictionary<string, GeofenceResult> mGeofenceResults;
      public IReadOnlyDictionary<string, GeofenceCircularRegion> Regions { get { return mRegions; } }
      public IReadOnlyDictionary<string, GeofenceResult> GeofenceResults { get { return mGeofenceResults; } }
      public bool IsMonitoring { get { return mRegions.Count > 0; } }

      public GeofenceImplementation()
      {

          mGeofenceResults = new Dictionary<string, GeofenceResult>();

          locationManager = new CLLocationManager();
          locationManager.DidStartMonitoringForRegion += DidStartMonitoringForRegion;
          locationManager.RegionEntered += RegionEntered;
          locationManager.RegionLeft +=RegionLeft;
          locationManager.Failed += OnFailure;
          locationManager.DidDetermineState += DidDetermineState;
          locationManager.LocationsUpdated += LocationsUpdated;
          string priorityType = "Balanced Power";
          switch(CrossGeofence.GeofencePriority)
          {
              case GeofencePriority.HighAccuracy:
                  priorityType = "High Accuracy";
                  locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
                  break;
              case GeofencePriority.AcceptableAccuracy:
                  priorityType = "Acceptable Accuracy";
                  locationManager.DesiredAccuracy = CLLocation.AccuracyNearestTenMeters;
                  break;
              case GeofencePriority.MediumAccuracy:
                  priorityType = "Medium Accuracy";
                  locationManager.DesiredAccuracy = CLLocation.AccuracyHundredMeters;
                  break;
              case GeofencePriority.LowAccuracy:
                  priorityType = "Low Accuracy";
                  locationManager.DesiredAccuracy = CLLocation.AccuracyKilometer;
                  break;
              case GeofencePriority.LowestAccuracy:
                  priorityType = "Lowest Accuracy";
                  locationManager.DesiredAccuracy = CLLocation.AccuracyThreeKilometers;
                  break;
              default:
                  locationManager.DesiredAccuracy = CLLocation.AccurracyBestForNavigation;
                  break;
          }
          System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2}", CrossGeofence.Id, "Location priority set to", priorityType));
    
          if(CrossGeofence.SmallestDisplacement>0)
          {
              locationManager.DistanceFilter = CrossGeofence.SmallestDisplacement;
              System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2} meters", CrossGeofence.Id, "Location smallest displacement set to", CrossGeofence.SmallestDisplacement));
          }

         

          if (locationManager.MonitoredRegions.Count > 0 && IsMonitoring)
          {

             
              
              NSSet monitoredRegions = locationManager.MonitoredRegions;
             

              foreach (CLCircularRegion region in monitoredRegions)
              {
                  if (!Regions.ContainsKey(region.Identifier))
                  {
                      locationManager.StopMonitoring(region);
                  }
                  else
                  {
                      locationManager.RequestState(region);
                  }
                 
              }

              locationManager.StartMonitoringSignificantLocationChanges();

              string message = string.Format("{0} - {1} {2} region(s)", CrossGeofence.Id, "Actually monitoring", locationManager.MonitoredRegions.Count);
              System.Diagnostics.Debug.WriteLine(message);
              
          }
      
          
      }

      void LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
      {
        CLLocation lastLocation = e.Locations[e.Locations.Length-1];
        
        if (Regions.Count > 20 )
        {
            IEnumerable<GeofenceCircularRegion> nearestRegions=Regions.Values.OrderBy(r => CalculateDistance(lastLocation.Coordinate.Latitude, lastLocation.Coordinate.Longitude, r.Latitude, r.Longitude)).Take(20);
            
            foreach (CLCircularRegion region in locationManager.MonitoredRegions)
            {
               locationManager.StopMonitoring(region);
            }

            foreach (GeofenceCircularRegion region in nearestRegions)
            {
                CLRegion cRegion = null;

                if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                {
                    cRegion = new CLCircularRegion(new CLLocationCoordinate2D(region.Latitude, region.Longitude), (region.Radius > locationManager.MaximumRegionMonitoringDistance) ? locationManager.MaximumRegionMonitoringDistance : region.Radius, region.Id);
                }
                else
                {
                    cRegion = new CLRegion(new CLLocationCoordinate2D(region.Latitude, region.Longitude), (region.Radius > locationManager.MaximumRegionMonitoringDistance) ? locationManager.MaximumRegionMonitoringDistance : region.Radius, region.Id);
                }


                cRegion.NotifyOnEntry = region.NotifyOnEntry;
                cRegion.NotifyOnExit = region.NotifyOnExit;

                locationManager.StartMonitoring(cRegion);
                locationManager.RequestState(cRegion);
            }

            string message = string.Format("{0} - {1}", CrossGeofence.Id, "Restarted monitoring to nearest 20 regions");
            System.Diagnostics.Debug.WriteLine(message);
        }
        else
        {
            //Check any current monitored regions not in loaded persistent regions and stop monitoring them
            foreach (CLCircularRegion region in locationManager.MonitoredRegions)
            {
                if (!Regions.ContainsKey(region.Identifier))
                {
                    locationManager.StopMonitoring(region);
                    string message = string.Format("{0} - Stopped monitoring region {1} wasn't in persistent loaded regions", CrossGeofence.Id, region.Identifier);
                    System.Diagnostics.Debug.WriteLine(message);
                }
            }
           
        }
      
        System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2},{3}", CrossGeofence.Id, "Location update",lastLocation.Coordinate.Latitude,lastLocation.Coordinate.Longitude));
       
      }

      public double CalculateDistance(double lat1,double lon1,double lat2,double lon2)
      {

          double R = 6372.8; // In kilometers
          double dLat = Math.PI * (lat2 - lat1) / 180.0; 
          double dLon = Math.PI * (lon2 - lon1) / 180.0; 
          lat1 = Math.PI * (lat1) / 180.0; 
          lat2 = Math.PI * (lat2) / 180.0; 

          double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
          double c = 2 * Math.Asin(Math.Sqrt(a));
          return (R * 2 * Math.Asin(Math.Sqrt(a))) * 1000; //meters
      }

    
      void DidDetermineState(object sender, CLRegionStateDeterminedEventArgs e)
      {
          switch (e.State)
          {
              case CLRegionState.Inside:
                  System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, "StartedRegion: " + e.Region));
                  OnRegionEntered(e.Region);
                  break;
              case CLRegionState.Outside:
                  break;
              default:
                  string message = string.Format("{0} - {1}", CrossGeofence.Id, "Unknown region state");
                  System.Diagnostics.Debug.WriteLine(message);
                  break;
          }
      }

      void OnFailure(object sender, NSErrorEventArgs e)
      {
          if (IsMonitoring)
          {
              StopMonitoring();
          }
          CrossGeofence.GeofenceListener.OnError(e.Error.LocalizedDescription);
      }

      void RegionEntered(object sender, CLRegionEventArgs e)
      {
          
          OnRegionEntered(e.Region);
        
      }
      async void OnRegionEntered(CLRegion region)
      {
          if (GeofenceResults.ContainsKey(region.Identifier) && GeofenceResults[region.Identifier].Transition == GeofenceTransition.Entered)
          {
              return;
          }

          if (!mGeofenceResults.ContainsKey(region.Identifier))
          {
              mGeofenceResults.Add(region.Identifier, new GeofenceResult()
              {
                  RegionId = region.Identifier
              });
          }
     
          mGeofenceResults[region.Identifier].Latitude = region.Center.Latitude;
          mGeofenceResults[region.Identifier].Latitude = region.Center.Longitude;
          mGeofenceResults[region.Identifier].LastEnterTime = DateTime.Now;
          mGeofenceResults[region.Identifier].LastExitTime = null;
          mGeofenceResults[region.Identifier].Transition = GeofenceTransition.Entered;
         
          CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeofenceResults[region.Identifier]);

          if (Regions.ContainsKey(region.Identifier) && Regions[region.Identifier].ShowNotification)
          {
              CreateNotification("View",string.IsNullOrEmpty(Regions[region.Identifier].NotificationEntryMessage) ? GeofenceResults[region.Identifier].ToString() : Regions[region.Identifier].NotificationEntryMessage);
          }



          if (Regions.ContainsKey(region.Identifier) && Regions[region.Identifier].NotifyOnStay && Regions[region.Identifier].StayedInThresholdDuration.TotalMilliseconds != 0)
          {
              await Task.Delay((int)Regions[region.Identifier].StayedInThresholdDuration.TotalMilliseconds);

              if (GeofenceResults[region.Identifier].LastExitTime == null && GeofenceResults[region.Identifier].Transition != GeofenceTransition.Stayed)
              {
                  mGeofenceResults[region.Identifier].Transition = GeofenceTransition.Stayed;
                  CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeofenceResults[region.Identifier]);

                  if (Regions[region.Identifier].ShowNotification)
                  {
                      CreateNotification("View", string.IsNullOrEmpty(Regions[region.Identifier].NotificationStayMessage) ? GeofenceResults[region.Identifier].ToString() : Regions[region.Identifier].NotificationStayMessage);
                  }


              }
          }
      }

      void RegionLeft(object sender, CLRegionEventArgs e)
      {
          if (!GeofenceResults.ContainsKey(e.Region.Identifier)||GeofenceResults[e.Region.Identifier].Transition != GeofenceTransition.Exited)
          {
              OnRegionLeft(e.Region);
              
          }
      }
      void OnRegionLeft(CLRegion region)
      {
          if (!mGeofenceResults.ContainsKey(region.Identifier))
          {
              mGeofenceResults.Add(region.Identifier, new GeofenceResult()
              {
                  RegionId = region.Identifier
              });
          }

          mGeofenceResults[region.Identifier].Latitude = region.Center.Latitude;
          mGeofenceResults[region.Identifier].Latitude = region.Center.Longitude;
          mGeofenceResults[region.Identifier].LastExitTime = DateTime.Now;
          mGeofenceResults[region.Identifier].Transition = GeofenceTransition.Exited;
         
          CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeofenceResults[region.Identifier]);

          if (Regions[region.Identifier].ShowNotification)
          {
              CreateNotification("View", string.IsNullOrEmpty(Regions[region.Identifier].NotificationExitMessage) ? GeofenceResults[region.Identifier].ToString() : Regions[region.Identifier].NotificationExitMessage);
          }
      }

      void DidStartMonitoringForRegion(object sender, CLRegionEventArgs e)
      {
          CrossGeofence.GeofenceListener.OnMonitoringStarted(e.Region.Identifier); 
      }

      public void StartMonitoring(IList<GeofenceCircularRegion> regions)
      {
          if (!CrossGeofence.IsInitialized)
          {
              throw NewGeofenceNotInitializedException();
          }

          RequestAlwaysAuthorization();

     
          if (!CLLocationManager.LocationServicesEnabled)
          {
              string message = string.Format("{0} - {1}", CrossGeofence.Id, "You need to enable Location Services");
              System.Diagnostics.Debug.WriteLine(message);
              CrossGeofence.GeofenceListener.OnError(message);
          }
          else if (CLLocationManager.Status == CLAuthorizationStatus.Denied || CLLocationManager.Status == CLAuthorizationStatus.Restricted)
          {
              string message = string.Format("{0} - {1}", CrossGeofence.Id, "You need to authorize Location Services");
              System.Diagnostics.Debug.WriteLine(message);
              CrossGeofence.GeofenceListener.OnError(message);

          }else if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
          {
              var settings = UIUserNotificationSettings.GetSettingsForTypes(
                UIUserNotificationType.Alert
                | UIUserNotificationType.Badge
                | UIUserNotificationType.Sound,
                new NSSet());
              UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);

              if (IsMonitoring)
              {
                  StopMonitoring();
              }

              locationManager.StartMonitoringSignificantLocationChanges();

              foreach (GeofenceCircularRegion region in regions)
              {
                  mRegions.Add(region.Id, region);
                  GeofenceStore.SharedInstance.Save(region);
              }

              IList<GeofenceCircularRegion> nearestRegions = GetCurrentRegions(regions);

              foreach (GeofenceCircularRegion region in nearestRegions)
              {
                  CLRegion cRegion=null;
                  

                  if (UIDevice.CurrentDevice.CheckSystemVersion(7, 0))
                  {
                      cRegion = new CLCircularRegion(new CLLocationCoordinate2D(region.Latitude, region.Longitude), (region.Radius > locationManager.MaximumRegionMonitoringDistance)?locationManager.MaximumRegionMonitoringDistance:region.Radius, region.Id);
                  }
                  else
                  {
                      cRegion = new CLRegion(new CLLocationCoordinate2D(region.Latitude, region.Longitude), (region.Radius > locationManager.MaximumRegionMonitoringDistance) ? locationManager.MaximumRegionMonitoringDistance : region.Radius, region.Id);
                  }


                  cRegion.NotifyOnEntry = region.NotifyOnEntry;
                  cRegion.NotifyOnExit = region.NotifyOnExit;

                
                
                  locationManager.StartMonitoring(cRegion);
                  locationManager.RequestState(cRegion);
              }
              
          }
          else
          {
              string message = string.Format("{0} - {1}",CrossGeofence.Id,"Plugin requires region monitoring, which is unavailable on this device");
              System.Diagnostics.Debug.WriteLine(message);

              CrossGeofence.GeofenceListener.OnError(message);
          }
          
      }

      public IList<GeofenceCircularRegion> GetCurrentRegions(IList<GeofenceCircularRegion> regions)
      {
          IList<GeofenceCircularRegion> nearestRegions = null;

          if (regions.Count > 20)
          {
              if (locationManager.Location != null)
              {
                  IEnumerable<GeofenceCircularRegion> nRegions = regions.OrderBy(r => CalculateDistance(locationManager.Location.Coordinate.Latitude, locationManager.Location.Coordinate.Longitude, r.Latitude, r.Longitude)).Take(20);

                  nearestRegions = nRegions.ToList();
              }
              else
              {
                  nearestRegions = regions.Take(20).ToList();
              }

          }
          else
          {
              nearestRegions = regions;
          }

          return nearestRegions;
      }

      public void StopMonitoring()
      {
          if (!CrossGeofence.IsInitialized)
          {
              throw NewGeofenceNotInitializedException();
          }

          GeofenceStore.SharedInstance.RemoveAll();

          foreach (CLCircularRegion region in locationManager.MonitoredRegions)
          {
              if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
              {
                  locationManager.StopMonitoring(region);
              }
          }
          locationManager.StopMonitoringSignificantLocationChanges();
          mRegions.Clear();
          mGeofenceResults.Clear();
          
   
          CrossGeofence.GeofenceListener.OnMonitoringStopped();
        
         
         
      }
      GeofenceNotInitializedException NewGeofenceNotInitializedException()
      {
          string description = string.Format("{0} - {1}", CrossGeofence.Id, "Plugin is not initialized. Should initialize before use with CrossGeofence Initialize method. Example:  CrossGeofence.Initialize<CrossGeofenceListener>()");

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
                  RemoveRegion(regionIdentifier);
                  GeofenceStore.SharedInstance.Remove(region.Identifier);
                  CrossGeofence.GeofenceListener.OnMonitoringStopped(regionIdentifier);

              }
              else
              {
                  System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, "Region Identifier: " + regionIdentifier + " isn't being monitored"));
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
                     RemoveRegion(regionIdentifier);
                     GeofenceStore.SharedInstance.Remove(region.Identifier);
                     CrossGeofence.GeofenceListener.OnMonitoringStopped(regionIdentifier);
                  }
                  else
                  {
                     System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, "Region Identifier: " + regionIdentifier + " isn't being monitored"));
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

          if (mGeofenceResults.ContainsKey(regionIdentifer))
          {
              mGeofenceResults.Remove(regionIdentifer);
          }
      }

      private CLRegion GetRegion(string identifier)
      {
          CLRegion region = null;
          foreach(CLRegion r in locationManager.MonitoredRegions)
          {
              if (r.Identifier.Equals(identifier, StringComparison.Ordinal))
              {
                  region = r;
                  break;
              }
          }
          return region;
      }

     

      void  CreateNotification(string  title,string  message)
      {
               UILocalNotification notification = new UILocalNotification();

               notification.AlertAction = title;
               notification.AlertBody = message;
               notification.HasAction = true;

               notification.SoundName = UILocalNotification.DefaultSoundName;
               #if __UNIFIED__
                   UIApplication.SharedApplication.PresentLocalNotificationNow(notification);
               #else
                   UIApplication.SharedApplication.PresentLocationNotificationNow(notification);
               #endif

      }
      void RequestAlwaysAuthorization()
      {
          CLAuthorizationStatus status = CLLocationManager.Status;
          if(status ==CLAuthorizationStatus.AuthorizedWhenInUse || status == CLAuthorizationStatus.Denied)
          {
              string title = (status == CLAuthorizationStatus.Denied) ? "Location services are off" : "Background location is not enabled";
              string message = "To use background location you must turn on 'Always' in the Location Services Settings";

              UIAlertView alertView = new UIAlertView(title, message, null, "Cancel", "Settings");
            

              alertView.Clicked += (sender, buttonArgs) => 
              {
                  if (buttonArgs.ButtonIndex == 1)
                  {
                      // Send the user to the Settings for this app
                      NSUrl settingsUrl = new NSUrl(UIApplication.OpenSettingsUrlString);
                      UIApplication.SharedApplication.OpenUrl(settingsUrl);

                  }
              };

              alertView.Show();
          }
          else if (status == CLAuthorizationStatus.NotDetermined)
          {
              locationManager.RequestAlwaysAuthorization();
          }
      }


  }
}
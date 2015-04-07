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

      private Dictionary<string, GeofenceCircularRegion> mRegions;
      private Dictionary<string, GeofenceResult> mGeofenceResults;
      public IReadOnlyDictionary<string, GeofenceCircularRegion> Regions { get { return mRegions; } }
      public IReadOnlyDictionary<string, GeofenceResult> GeofenceResults { get { return mGeofenceResults; } }
      public bool IsMonitoring { get { return mRegions.Count > 0; } }

      private GeofenceTransition lastGeofenceTransition=GeofenceTransition.Unknown;

      public GeofenceImplementation()
      {
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

          }
          else if (!CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
          {
              string message = string.Format("{0} - {1}", CrossGeofence.Id, "Plugin requires region monitoring, which is unavailable on this device");
              System.Diagnostics.Debug.WriteLine(message);

              CrossGeofence.GeofenceListener.OnError(message);
          }

          mRegions = new Dictionary<string, GeofenceCircularRegion>();
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

          if (CrossGeofence.EnableLocalNotifications)
          {
              var settings = UIUserNotificationSettings.GetSettingsForTypes(
                UIUserNotificationType.Alert
                | UIUserNotificationType.Badge
                | UIUserNotificationType.Sound,
                new NSSet());
              UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
          }

          NSSet monitoredRegions = locationManager.MonitoredRegions;
          foreach (CLCircularRegion region in monitoredRegions)
          {
              mRegions.Add(region.Identifier, new GeofenceCircularRegion()
              {
                  Id=region.Identifier,
                  Latitude=region.Center.Latitude,
                  Longitude=region.Center.Longitude,
                  Radius=region.Radius,
                  NotifyOnEntry=region.NotifyOnEntry,
                  NotifyOnExit=region.NotifyOnExit
              });
          }

          //locationManager.StartMonitoringSignificantLocationChanges();
      }

      void LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
      {
        
        NSSet monitoredRegions = locationManager.MonitoredRegions;

        if (monitoredRegions.Count > 0)
        {
            foreach(CLRegion region in monitoredRegions)
            {
               
                string identifier = region.Identifier;
                CLLocation lastLocation = e.Locations[e.Locations.Length-1];
                CLLocationCoordinate2D centerCoords = region.Center;
                CLLocationCoordinate2D currentCoords = new CLLocationCoordinate2D(lastLocation.Coordinate.Latitude, lastLocation.Coordinate.Longitude);

                double currentLocationDistance = CalculateDistance(currentCoords, centerCoords);
                 if(currentLocationDistance < region.Radius)
                 {
                        // NSLog(@"Invoking didEnterRegion Manually for region: %@",identifer);
 
                        //stop Monitoring Region temporarily
                        // [locationManager stopMonitoringForRegion:region];
 
                         //[self locationManager:locationManager didEnterRegion:region];
                         //start Monitoing Region again.
                         //[locationManager startMonitoringForRegion:region];
                         locationManager.StopMonitoring(region);
                         
                         locationManager.StartMonitoring(region);
                 }
            }
        }
       
      }
      double CalculateDistance(CLLocationCoordinate2D coordinate1,CLLocationCoordinate2D coordinate2)
      {
          int nRadius = 6371; // Earth's radius in Kilometers
          double latDiff = (coordinate2.Latitude - coordinate1.Latitude) * (Math.PI / 180);
          double lonDiff = (coordinate2.Longitude - coordinate1.Longitude) * (Math.PI / 180);
          double lat1InRadians = coordinate1.Latitude * (Math.PI / 180);
          double lat2InRadians = coordinate2.Latitude * (Math.PI / 180);
          double nA = Math.Pow(Math.Sin(latDiff / 2), 2) + Math.Cos(lat1InRadians) * Math.Cos(lat2InRadians) * Math.Pow(Math.Sin(lonDiff / 2), 2);
          double nC = 2 * Math.Atan2(Math.Sqrt(nA), Math.Sqrt(1 - nA));
          double nD = nRadius * nC;
          return (nD * 1000);
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
          if (lastGeofenceTransition != GeofenceTransition.Entered)
          {
              OnRegionEntered(e.Region);
              
          }
        
      }
      async void OnRegionEntered(CLRegion region)
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
          mGeofenceResults[region.Identifier].LastEnterTime = DateTime.Now;
          mGeofenceResults[region.Identifier].LastExitTime = null;
          mGeofenceResults[region.Identifier].Transition = GeofenceTransition.Entered;
          lastGeofenceTransition = GeofenceTransition.Entered;
          CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeofenceResults[region.Identifier]);
          
          CreateNotification("View",string.Format("{0} {1} {2}", GeofenceResult.GetTransitionString(mGeofenceResults[region.Identifier].Transition), "geofence region:",region.Identifier));
          if (mRegions.ContainsKey(region.Identifier) && CrossGeofence.StayedInDuration != 0)
          {
              await Task.Delay(CrossGeofence.StayedInDuration);

              if (mGeofenceResults[region.Identifier].LastExitTime == null)
              {
                  mGeofenceResults[region.Identifier].Transition = GeofenceTransition.Stayed;
                  CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeofenceResults[region.Identifier]);
                  lastGeofenceTransition = GeofenceTransition.Stayed;
              }
          }
      }

      void RegionLeft(object sender, CLRegionEventArgs e)
      {
          if (lastGeofenceTransition != GeofenceTransition.Exited)
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
          lastGeofenceTransition = GeofenceTransition.Exited;
          CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeofenceResults[region.Identifier]);

          CreateNotification("View", string.Format("{0} {1} {2}", GeofenceResult.GetTransitionString(mGeofenceResults[region.Identifier].Transition), "geofence region:", region.Identifier));
          
      }

      void DidStartMonitoringForRegion(object sender, CLRegionEventArgs e)
      {
          CrossGeofence.GeofenceListener.OnMonitoringStarted();
          //locationManager.RequestState(e.Region);
     
      }

      public void StartMonitoring(IList<GeofenceCircularRegion> regions)
      {
          if (!CrossGeofence.IsInitialized)
          {
              throw NewGeofenceNotInitializedException();
          }

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
              if (IsMonitoring)
              {
                  StopMonitoring();
              }

              foreach (GeofenceCircularRegion region in regions)
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

                  mRegions.Add(region.Id, region);
                  locationManager.StartMonitoring(cRegion);
              }

          }
          else
          {
              string message = string.Format("{0} - {1}",CrossGeofence.Id,"Plugin requires region monitoring, which is unavailable on this device");
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

          foreach (CLCircularRegion region in locationManager.MonitoredRegions)
          {
              if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
              {
                  locationManager.StopMonitoring(region);
               
              }
          }
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

      public void StartMonitoring(GeofenceCircularRegion region)
      {
          if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
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

              if (!mRegions.ContainsKey(region.Id))
              {
                  mRegions.Add(region.Id, region);
                  locationManager.StartMonitoring(cRegion);
              }
              else
              {
                  System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, "Region: "+ region.Id +" already been monitored"));
              }

             
          }

         
          

      }
      void  CreateNotification(string  title,string  message)
      {
           if(CrossGeofence.EnableLocalNotifications)
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
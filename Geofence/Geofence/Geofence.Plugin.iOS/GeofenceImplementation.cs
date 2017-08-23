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
      /// <summary>
      /// Monitored regions
      /// </summary>
      public IReadOnlyDictionary<string, GeofenceCircularRegion> Regions { get { return mRegions; } }
      /// <summary>
      /// Geofence results
      /// </summary>
      public IReadOnlyDictionary<string, GeofenceResult> GeofenceResults { get { return mGeofenceResults; } }
      /// <summary>
      /// Checks if is monitoring regions
      /// </summary>
      public bool IsMonitoring { get { return mRegions.Count > 0; } }
      /// <summary>
      /// This checks if we are currently prompting for location permissions to avoid the double prompt from multiple simultaneous regions
      /// </summary>
      bool isPromptingLocationPermission;

      private GeofenceLocation lastKnownGeofenceLocation;

      /// <summary>
      /// Set this flag to false if the application is already asking the user for permissions to send Notifications.
      /// </summary>
      /// <value><c>true</c> if request notification permission; otherwise, <c>false</c>.</value>
      public bool RequestNotificationPermission { get; set; }

      /// <summary>
      /// Set this flag to false if the application is already prompting the user to use Location Services.
      /// </summary>
      /// <value><c>true</c> if request location permission; otherwise, <c>false</c>.</value>
      public bool RequestLocationPermission { get; set; }

      private const string ViewAction = "View";
      /// <summary>
      /// Last known location
      /// </summary>
      public GeofenceLocation LastKnownLocation { get { return lastKnownGeofenceLocation; } }
      /// <summary>
      /// Geofence plugin iOS implementation
      /// </summary>
      public GeofenceImplementation()
      {

          mGeofenceResults = new Dictionary<string, GeofenceResult>();

          using (var pool = new NSAutoreleasePool())
          {
              pool.InvokeOnMainThread(() => {
                  locationManager = new CLLocationManager();
                  locationManager.DidStartMonitoringForRegion += DidStartMonitoringForRegion;
                  locationManager.RegionEntered += RegionEntered;
                  locationManager.RegionLeft += RegionLeft;
                  locationManager.Failed += OnFailure;
                  locationManager.DidDetermineState += DidDetermineState;
                  locationManager.LocationsUpdated += LocationsUpdated;
              });
          } 
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
                  //If not on regions remove on startup since that region was set not persistent
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

          SetLastKnownLocation(locationManager.Location);
          
      }
      void SetLastKnownLocation(CLLocation location)
      {
          if (location != null)
          {
              if (lastKnownGeofenceLocation == null)
              {
                  lastKnownGeofenceLocation = new GeofenceLocation();
              }
             
              lastKnownGeofenceLocation.Latitude = location.Coordinate.Latitude;
              lastKnownGeofenceLocation.Longitude = location.Coordinate.Longitude;
              lastKnownGeofenceLocation.Accuracy = location.HorizontalAccuracy;
              DateTime referenceDate = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(2001, 1, 1, 0, 0, 0));

              lastKnownGeofenceLocation.Date = referenceDate.AddSeconds(location.Timestamp.SecondsSinceReferenceDate);

              CrossGeofence.GeofenceListener.OnLocationChanged(lastKnownGeofenceLocation);
          }

      }
      void LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
      {
        CLLocation lastLocation = e.Locations[e.Locations.Length-1];

        SetLastKnownLocation(lastLocation);

        if (Regions.Count > 20 && locationManager.MonitoredRegions.Count == 20)
        {

            RecalculateRegions();

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
      /// <summary>
      /// Calculates distance between two locations
      /// </summary>
      /// <param name="lat1"></param>
      /// <param name="lon1"></param>
      /// <param name="lat2"></param>
      /// <param name="lon2"></param>
      /// <returns></returns>
      public double CalculateDistance(double lat1,double lon1,double lat2,double lon2)
      {

          double R = 6372.8; // In kilometers
          double dLat = Math.PI * (lat2 - lat1) / 180.0; 
          double dLon = Math.PI * (lon2 - lon1) / 180.0; 
          lat1 = Math.PI * (lat1) / 180.0; 
          lat2 = Math.PI * (lat2) / 180.0; 

          double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
          double c = 2 * Math.Asin(Math.Sqrt(a));
          return (R * c) * 1000; //meters
      }

    
      void DidDetermineState(object sender, CLRegionStateDeterminedEventArgs e)
      {
          switch (e.State)
          {
              case CLRegionState.Inside:
                  System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, "InsideRegion: " + e.Region));
                  OnRegionEntered(e.Region);
                  break;
              case CLRegionState.Outside:
                  System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, "OutsideRegion: " + e.Region));
                  OnRegionLeft(e.Region);
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
              StopMonitoringAllRegions();
          }
          CrossGeofence.GeofenceListener.OnError(e.Error.LocalizedDescription);
      }

      void RegionEntered(object sender, CLRegionEventArgs e)
      {
          
          OnRegionEntered(e.Region);
        
      }
      void OnRegionEntered(CLRegion region)
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

          if (LastKnownLocation != null)
          {
              mGeofenceResults[region.Identifier].Latitude = LastKnownLocation.Latitude;
              mGeofenceResults[region.Identifier].Longitude = LastKnownLocation.Longitude;
              mGeofenceResults[region.Identifier].Accuracy = LastKnownLocation.Accuracy;
          }
          else
          {
              mGeofenceResults[region.Identifier].Latitude = region.Center.Latitude;
              mGeofenceResults[region.Identifier].Longitude = region.Center.Longitude;
              mGeofenceResults[region.Identifier].Accuracy = region.Radius;
          }
     
          mGeofenceResults[region.Identifier].LastEnterTime = DateTime.Now;
          mGeofenceResults[region.Identifier].LastExitTime = null;
          mGeofenceResults[region.Identifier].Transition = GeofenceTransition.Entered;
          if (mRegions.ContainsKey(region.Identifier))
              region.NotifyOnEntry = mRegions[region.Identifier].NotifyOnEntry;
          if(region.NotifyOnEntry)
          {
              CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeofenceResults[region.Identifier]);

              if (Regions.ContainsKey(region.Identifier) && Regions[region.Identifier].ShowNotification && Regions[region.Identifier].ShowEntryNotification)
              {
                  CreateNotification(ViewAction, string.IsNullOrEmpty(Regions[region.Identifier].NotificationEntryMessage) ? GeofenceResults[region.Identifier].ToString() : Regions[region.Identifier].NotificationEntryMessage);
              }

          }


          //Checks if device has stayed asynchronously
          CheckIfStayed(region.Identifier);

        
      }
      /// <summary>
      /// Checks if has passed to stayed state
      /// </summary>
      /// <param name="regionId"></param>
      /// <returns></returns>
      public async Task CheckIfStayed(string regionId)
      {
          if (GeofenceRegionExists(regionId) && CrossGeofence.Current.Regions[regionId].NotifyOnStay && CrossGeofence.Current.GeofenceResults[regionId].Transition == GeofenceTransition.Entered && CrossGeofence.Current.Regions[regionId].StayedInThresholdDuration.TotalMilliseconds != 0)
          {
              await Task.Delay((int)CrossGeofence.Current.Regions[regionId].StayedInThresholdDuration.TotalMilliseconds);

              if (GeofenceRegionExists(regionId) && CrossGeofence.Current.GeofenceResults[regionId].LastExitTime == null && CrossGeofence.Current.GeofenceResults[regionId].Transition != GeofenceTransition.Stayed)
              {
                  CrossGeofence.Current.GeofenceResults[regionId].Transition = GeofenceTransition.Stayed;

                  CrossGeofence.GeofenceListener.OnRegionStateChanged(CrossGeofence.Current.GeofenceResults[regionId]);

                  if (CrossGeofence.Current.Regions[regionId].ShowNotification && CrossGeofence.Current.Regions[regionId].ShowStayNotification)
                  {
                      CreateNotification(ViewAction, string.IsNullOrEmpty(CrossGeofence.Current.Regions[regionId].NotificationStayMessage) ? CrossGeofence.Current.GeofenceResults[regionId].ToString() : CrossGeofence.Current.Regions[regionId].NotificationStayMessage);
                  }

              }
          }
      }

      /// <summary>
      /// Checks if a GeofenceRegion exists in the monitored regions
      /// </summary>
      /// <returns><c>true</c>, if it exists, <c>false</c> otherwise.</returns>
      /// <param name="regionId">Region identifier.</param>
      bool GeofenceRegionExists(string regionId)
      {
          return CrossGeofence.Current.GeofenceResults.ContainsKey (regionId) && CrossGeofence.Current.Regions.ContainsKey (regionId);
      }

      void RegionLeft(object sender, CLRegionEventArgs e)
      {
          OnRegionLeft(e.Region);
      }

      void OnRegionLeft(CLRegion region)
      {
          if (GeofenceResults.ContainsKey(region.Identifier) && GeofenceResults[region.Identifier].Transition == GeofenceTransition.Exited)
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

          if(LastKnownLocation!=null)
          {
              mGeofenceResults[region.Identifier].Latitude = LastKnownLocation.Latitude;
              mGeofenceResults[region.Identifier].Longitude = LastKnownLocation.Longitude;
              mGeofenceResults[region.Identifier].Accuracy = LastKnownLocation.Accuracy;
          }
          else
          {
              mGeofenceResults[region.Identifier].Latitude = region.Center.Latitude;
              mGeofenceResults[region.Identifier].Longitude = region.Center.Longitude;
              mGeofenceResults[region.Identifier].Accuracy = region.Radius;
          }
        
          mGeofenceResults[region.Identifier].LastExitTime = DateTime.Now;
          mGeofenceResults[region.Identifier].Transition = GeofenceTransition.Exited;
         
          CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeofenceResults[region.Identifier]);

          if (Regions.ContainsKey(region.Identifier) && Regions[region.Identifier].ShowNotification && Regions[region.Identifier].ShowExitNotification)
          {
              CreateNotification(ViewAction, string.IsNullOrEmpty(Regions[region.Identifier].NotificationExitMessage) ? GeofenceResults[region.Identifier].ToString() : Regions[region.Identifier].NotificationExitMessage);
          }
      }

      void DidStartMonitoringForRegion(object sender, CLRegionEventArgs e)
      {
          CrossGeofence.GeofenceListener.OnMonitoringStarted(e.Region.Identifier); 
      }
      /// <summary>
      /// Checks if is available for monitoring
      /// </summary>
      /// <returns></returns>
      public bool AvailableForMonitoring()
      {
          bool retVal = false;
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
          else if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
          {
              if (RequestNotificationPermission)
              {
                  using (var pool = new NSAutoreleasePool())
                  {
                      pool.InvokeOnMainThread(() => {
                          var settings = UIUserNotificationSettings.GetSettingsForTypes(
                                             UIUserNotificationType.Alert
                                             | UIUserNotificationType.Badge
                                             | UIUserNotificationType.Sound,
                                             new NSSet());
                          UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
                      });
                  }
              }
              retVal = true;
          }
          else
          {
              string message = string.Format("{0} - {1}", CrossGeofence.Id, "Not available for monitoring");
              System.Diagnostics.Debug.WriteLine(message);
              CrossGeofence.GeofenceListener.OnError(message);
          }

          

          return retVal;

      }
      /// <summary>
      /// Starts monitoring region
      /// </summary>
      /// <param name="region"></param>
      public void StartMonitoring(GeofenceCircularRegion region)
      {

          if(AvailableForMonitoring())
          {

              if (!mRegions.ContainsKey(region.Id))
              {
                      mRegions.Add(region.Id, region);
                      
              }else{
                      mRegions[region.Id]=region;
              }
              GeofenceStore.SharedInstance.Save(region);

              if (Regions.Count > 20 && locationManager.MonitoredRegions.Count == 20)
              {
                  
                  RecalculateRegions();
              }
              else
              {
                  AddRegion(region);
              }

              locationManager.StartMonitoringSignificantLocationChanges();
              
          }
      }

      void RecalculateRegions()
      {
          IList<GeofenceCircularRegion> regions = Regions.Values.ToList();
          //Stop all monitored regions
          foreach (CLCircularRegion region in locationManager.MonitoredRegions)
          {

              locationManager.StopMonitoring(region);
          }

          IList<GeofenceCircularRegion> nearestRegions = GetCurrentRegions(regions);

          foreach (GeofenceCircularRegion region in nearestRegions)
          {
              AddRegion(region);
          }

          string message = string.Format("{0} - {1}", CrossGeofence.Id, "Restarted monitoring to nearest 20 regions");
          System.Diagnostics.Debug.WriteLine(message);
 
      }
      void AddRegion(GeofenceCircularRegion region)
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


          cRegion.NotifyOnEntry = region.NotifyOnEntry || region.NotifyOnStay;
          cRegion.NotifyOnExit = region.NotifyOnExit;


          locationManager.StartMonitoring(cRegion);

          // Request state for this region, putting request behind a timer per thread: http://stackoverflow.com/questions/24543814/diddeterminestate-not-always-called
          Task.Run(async() => {
              await Task.Delay(TimeSpan.FromSeconds(2));
              locationManager.RequestState(cRegion);
          });
      }
      /// <summary>
      /// Start monitoring regions
      /// </summary>
      /// <param name="regions"></param>
      public void StartMonitoring(IList<GeofenceCircularRegion> regions)
      {

          if (AvailableForMonitoring())
          {
             
              
              foreach (var region in regions)
              {
                  if (!mRegions.ContainsKey(region.Id))
                  {
                      mRegions.Add(region.Id, region);
                      
                  }else{
                      mRegions[region.Id]=region;
                  }

                  GeofenceStore.SharedInstance.Save(region);
              }
              

              if (Regions.Count > 20 && locationManager.MonitoredRegions.Count == 20)
              {

                  RecalculateRegions();
              }
              else
              {
                  foreach (var region in regions)
                  {
                      AddRegion(region);
                  }
                  
              }
              locationManager.StartMonitoringSignificantLocationChanges();

          }
          
      }
      /// <summary>
      /// Get current 20 monitored regions.
      /// </summary>
      /// <param name="regions"></param>
      /// <returns></returns>
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
      /// <summary>
      /// Stops monitoring all regions
      /// </summary>
      public void StopMonitoringAllRegions()
      {
          if (AvailableForMonitoring())
          {
              GeofenceStore.SharedInstance.RemoveAll();

              foreach (CLCircularRegion region in locationManager.MonitoredRegions)
              {

                  locationManager.StopMonitoring(region);
              }
              locationManager.StopMonitoringSignificantLocationChanges();
              mRegions.Clear();
              mGeofenceResults.Clear();
              CrossGeofence.GeofenceListener.OnMonitoringStopped();
          }
         
         
      }
      /// <summary>
      /// Stops monitoring region
      /// </summary>
      /// <param name="regionIdentifier"></param>
      public void StopMonitoring(string regionIdentifier)
      {
          if (CLLocationManager.IsMonitoringAvailable(typeof(CLRegion)))
          {
              RemoveRegionMonitoring(regionIdentifier);
              
              CrossGeofence.GeofenceListener.OnMonitoringStopped(regionIdentifier);

              if (mRegions.Count == 0)
              {
                  CrossGeofence.GeofenceListener.OnMonitoringStopped();
              }
             
       
          }


      }
      /// <summary>
      /// Stop monitoring regions
      /// </summary>
      /// <param name="regionIdentifiers"></param>
      public void StopMonitoring(IList<string> regionIdentifiers)
      {
          if (AvailableForMonitoring())
          {

              foreach (string regionIdentifier in regionIdentifiers)
              {
                  StopMonitoring(regionIdentifier);

              }
          }
   
      }

      private void RemoveRegionMonitoring(string regionIdentifier)
      {
          if (mRegions.ContainsKey(regionIdentifier))
          {
              mRegions.Remove(regionIdentifier);
          }

          if (mGeofenceResults.ContainsKey(regionIdentifier))
          {
              mGeofenceResults.Remove(regionIdentifier);
          }
          GeofenceStore.SharedInstance.Remove(regionIdentifier);

          var region = GetRegion(regionIdentifier);
          if (region != null)
          {
              locationManager.StopMonitoring(region);
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
               // Do nothing if we have no notification permission at this time, or we will get a buildup of stale notifications
               if (UIApplication.SharedApplication.CurrentUserNotificationSettings.Types == UIUserNotificationType.None)
                   return;
            
               var notification = new UILocalNotification();

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
          if (!RequestLocationPermission)
              return;

          if (isPromptingLocationPermission)
              return;
          isPromptingLocationPermission = true;

          CLAuthorizationStatus status = CLLocationManager.Status;
          if(status ==CLAuthorizationStatus.AuthorizedWhenInUse || status == CLAuthorizationStatus.Denied)
          {
              string title = (status == CLAuthorizationStatus.Denied) ? "Location services are off" : "Background location is not enabled";
              string message = "To use background location you must turn on 'Always' in the Location Services Settings";

              using (var pool = new NSAutoreleasePool())
              {
                  pool.InvokeOnMainThread(() => {
                      UIAlertView alertView = new UIAlertView(title, message, null, "Cancel", "Settings");

                      alertView.Clicked += (sender, buttonArgs) => 
                      {
                          if (buttonArgs.ButtonIndex == 1)
                          {
                              // Send the user to the Settings for this app
                              NSUrl settingsUrl = new NSUrl(UIApplication.OpenSettingsUrlString);
                              UIApplication.SharedApplication.OpenUrl(settingsUrl);
                          }
                          isPromptingLocationPermission = false;
                      };

                      alertView.Show();
                  });
              }
          }
          else if (status == CLAuthorizationStatus.NotDetermined)
          {
              locationManager.RequestAlwaysAuthorization();
          }
      }

      /// <summary>
      /// For iOS, it's AuthorizedAlways or it's not authorized.
      /// </summary>
      /// <returns>true</returns>
      /// <c>false</c>
      /// <param name="returnAction">Return action.</param>
      public void IsLocationEnabled(Action<bool> returnAction)
      {
          returnAction(CLLocationManager.Status == CLAuthorizationStatus.AuthorizedAlways);
      }


  }
}
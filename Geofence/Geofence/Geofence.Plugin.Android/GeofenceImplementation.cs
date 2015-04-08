using Geofence.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using Android.Util;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.App;
using Android.Content;
using Android.OS;
using System.Linq;
using Android.Support.V4.App;
using Android.Media;
using Java.Lang;
using Android.Text;
using System.Runtime.Remoting.Messaging;
using Java.Interop;
using System.Collections.ObjectModel;

namespace Geofence.Plugin
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    /// 
    public class GeofenceImplementation : Java.Lang.Object, IGeofence, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener, IResultCallback
    {

      public const string GeoReceiverAction = "ACTION_RECEIVE_GEOFENCE";
  
      private Dictionary<string,GeofenceCircularRegion> mRegions= GeofenceStore.SharedInstance.GetGeofenceRegions();

      private  Dictionary<string, GeofenceResult> mGeofenceResults= new Dictionary<string, GeofenceResult>();

      private PendingIntent mGeofencePendingIntent;

      private IGoogleApiClient mGoogleApiClient;

      // Defines the allowable request types
	  public enum RequestType { Add,Update, Default }

      public IReadOnlyDictionary<string, GeofenceCircularRegion> Regions { get { return mRegions; } }

      public IReadOnlyDictionary<string, GeofenceResult> GeofenceResults { get { return mGeofenceResults; } }

      public RequestType CurrentRequestType { get; set; }
  
      public bool IsMonitoring { get { return mRegions.Count > 0; } }
      
        //IsMonitoring?RequestType.Add:

      private PendingIntent GeofenceTransitionPendingIntent
      {
          get
          {
              // If the PendingIntent already exists
              if (mGeofencePendingIntent==null)
              {

                  //var intent = new Intent(Android.App.Application.Context, typeof(GeofenceBroadcastReceiver));
                  // intent.SetAction(string.Format("{0}.{1}", Android.App.Application.Context.PackageName, GeoReceiverAction));
                  var intent = new Intent(string.Format("{0}.{1}", Android.App.Application.Context.PackageName, GeoReceiverAction));
                  mGeofencePendingIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
              }
              return mGeofencePendingIntent;
          }
      }

      public GeofenceImplementation()
	  {

          CurrentRequestType = RequestType.Default;

          InitializeGoogleAPI();

          if (CrossGeofence.EnableMonitoringRestore)
          {
              RestoreMonitoring();
          }

	  }
      /// <summary>
      /// Starts geofence monitoring on specified regions
      /// </summary>
      /// <param name="regions"></param>
      public void StartMonitoring(IList<GeofenceCircularRegion> regions)
      {
          StartMonitoring(regions, true);

      }

      public void RestoreMonitoring()
      {
          if(IsMonitoring)
          {
              StartMonitoring(mRegions.Values.ToList(), false);
              string message = string.Format("{0} - {1}", CrossGeofence.Id, "Monitoring was restored");
          }
          else
          {
              mRegions.Clear();
              mGeofenceResults.Clear();
              GeofenceStore.SharedInstance.ClearGeofenceRegions();
          }
     
      }

      public void StartMonitoring(IList<GeofenceCircularRegion> regions,bool clear)
      {
          
          if (!CrossGeofence.IsInitialized)
          {
              throw NewGeofenceNotInitializedException();
          }
         
          //Check if location services are enabled
          if (!((Android.Locations.LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService)).IsProviderEnabled(Android.Locations.LocationManager.GpsProvider))
          {
              string message = string.Format("{0} - {1}", CrossGeofence.Id, "You need to enable Location Services");
              System.Diagnostics.Debug.WriteLine(message);
              CrossGeofence.GeofenceListener.OnError(message);
              return;
          }
       
          //If monitoring stop
          if (IsMonitoring && clear)
          {
              mRegions.Clear();
              mGeofenceResults.Clear();
              GeofenceStore.SharedInstance.ClearGeofenceRegions();
              Android.Gms.Location.LocationServices.GeofencingApi.RemoveGeofences(mGoogleApiClient, GeofenceTransitionPendingIntent).SetResultCallback(this);
        
          }
         
          foreach (GeofenceCircularRegion region in regions)
          {
              if (!mRegions.ContainsKey(region.Id))
              {
                  mRegions.Add(region.Id, region);
              }

          }
          
          //If connected to google play services then add regions
          if (mGoogleApiClient.IsConnected)
          {
            
              AddGeofences();
             
        
            }
            else
            {
                //If not connection then connect
                if (!mGoogleApiClient.IsConnecting)
                {
                    mGoogleApiClient.Connect();

                }
                //Request to add geofence regions once connected
                CurrentRequestType = RequestType.Add;

            }
        }
        public void AddGeofenceResult(string identifier)
        {
             mGeofenceResults.Add(identifier, new GeofenceResult()
             {
                            RegionId = identifier,
                            Transition = GeofenceTransition.Unknown
              });
        }
        private void AddGeofences()
        {
            try
            {
                List<Android.Gms.Location.IGeofence> geofenceList = new List<Android.Gms.Location.IGeofence>();
                var regions = Regions.Values;
                foreach (GeofenceCircularRegion region in regions)
                {
                    int transitionTypes = 0;
                    
                    if(region.NotifyOnStay)
                    {
                        transitionTypes |= Android.Gms.Location.Geofence.GeofenceTransitionDwell;
                    }

                    if(region.NotifyOnEntry)
                    {
                        transitionTypes |= Android.Gms.Location.Geofence.GeofenceTransitionEnter;
                    }

                    if (region.NotifyOnExit)
                    {
                        transitionTypes |= Android.Gms.Location.Geofence.GeofenceTransitionExit;
                    }

                    geofenceList.Add(new Android.Gms.Location.GeofenceBuilder()
                    .SetRequestId(region.Id)
                    .SetCircularRegion(region.Latitude, region.Longitude, (float)region.Radius)
                    .SetLoiteringDelay(CrossGeofence.StayedInDuration)
                    //.SetNotificationResponsiveness(mNotificationResponsivness)
                    .SetExpirationDuration(Android.Gms.Location.Geofence.NeverExpire)
                     //Android.Gms.Location.Geofence.GeofenceTransitionEnter | Android.Gms.Location.Geofence.GeofenceTransitionDwell | Android.Gms.Location.Geofence.GeofenceTransitionExit
                    .SetTransitionTypes(transitionTypes)
                    .Build());

                    if (GeofenceStore.SharedInstance.GetGeofenceRegion(region.Id)==null)
                    {
                        GeofenceStore.SharedInstance.SetGeofenceRegion(region);
                    }
                    
                }
                Android.Gms.Location.GeofencingRequest request = new Android.Gms.Location.GeofencingRequest.Builder().SetInitialTrigger(Android.Gms.Location.GeofencingRequest.InitialTriggerEnter).AddGeofences(geofenceList).Build();
            
                Android.Gms.Location.LocationServices.GeofencingApi.AddGeofences(mGoogleApiClient, request, GeofenceTransitionPendingIntent).SetResultCallback(this);

                CurrentRequestType = RequestType.Default;

            }catch(Java.Lang.Exception ex1)
            {
                string message = string.Format("{0} - Error: {1}", CrossGeofence.Id, ex1.ToString());
                System.Diagnostics.Debug.WriteLine(message);
                CrossGeofence.GeofenceListener.OnError(message);
            }
            catch (System.Exception ex2)
            {
                string message = string.Format("{0} - Error: {1}", CrossGeofence.Id, ex2.ToString());
                System.Diagnostics.Debug.WriteLine(message);
                CrossGeofence.GeofenceListener.OnError(message);
            }
            
        }

        /// <summary>
        /// Stops geofence monitoring
        /// </summary>
        public void StopMonitoring()
        {
            if (!CrossGeofence.IsInitialized)
            {
                throw NewGeofenceNotInitializedException();
            }
            GeofenceStore.SharedInstance.ClearGeofenceRegions();
            mRegions.Clear();
            mGeofenceResults.Clear();
            Android.Gms.Location.LocationServices.GeofencingApi.RemoveGeofences(mGoogleApiClient, GeofenceTransitionPendingIntent).SetResultCallback(this);
            
            StopLocationUpdates();
            

            mGoogleApiClient.Disconnect();

            CrossGeofence.GeofenceListener.OnMonitoringStopped();

           
            
        }

        private void InitializeGoogleAPI()
        {
            int queryResult = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(Android.App.Application.Context);

            if (queryResult == ConnectionResult.Success)
            {
                if(mGoogleApiClient==null)
                {
                    mGoogleApiClient = new GoogleApiClientBuilder(Android.App.Application.Context).AddApi(Android.Gms.Location.LocationServices.Api).AddConnectionCallbacks(this).AddOnConnectionFailedListener(this).Build();
                    string message = string.Format("{0} - {1}", CrossGeofence.Id, "Google Play services is available.");
                    System.Diagnostics.Debug.WriteLine(message);
                }

                if (!mGoogleApiClient.IsConnected)
                {
                    mGoogleApiClient.Connect();

                }

            }
            else
            {
            
                string message = string.Format("{0} - {1}", CrossGeofence.Id, "Google Play services is unavailable.");
                System.Diagnostics.Debug.WriteLine(message);
                CrossGeofence.GeofenceListener.OnError(message);
            }
        }

        public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
        {
            int errorCode = result.ErrorCode;
            string message = string.Format("{0} - {1} {2}", CrossGeofence.Id, "Connection to Google Play services failed with error code ", errorCode);
            System.Diagnostics.Debug.WriteLine(message);
            CrossGeofence.GeofenceListener.OnError(message);
      
        }

        public void OnConnected(Bundle connectionHint)
        {
            // Use CurrentRequestType to determine what action to take. Only Add used in this sample
            if (CurrentRequestType == RequestType.Add)
            {
                AddGeofences();
          
            }

            StartLocationUpdates();
            

        }

        public void OnResult(Java.Lang.Object result)
        {
            var res = result.JavaCast<IResult>();
          
            int statusCode = res.Status.StatusCode;
            string message = string.Empty;

            switch (res.Status.StatusCode)
            {
                case Android.Gms.Location.GeofenceStatusCodes.SuccessCache:
                case Android.Gms.Location.GeofenceStatusCodes.Success:
                    if(CurrentRequestType!=RequestType.Update)
                    {
                        message = string.Format("{0} - {1}", CrossGeofence.Id, "Successfully added Geofence.");
                        
                        foreach(GeofenceCircularRegion region in Regions.Values)
                        {
                            CrossGeofence.GeofenceListener.OnMonitoringStarted(region.Id);
                        }
                    }
                    else
                    {
                        message = string.Format("{0} - {1}", CrossGeofence.Id, "Geofence Update Received");
                    }
                    
                    break;
                case Android.Gms.Location.GeofenceStatusCodes.Error:
                    message = string.Format("{0} - {1}", CrossGeofence.Id, "Error adding Geofence.");
                    break;
                case Android.Gms.Location.GeofenceStatusCodes.GeofenceTooManyGeofences:
                    message = string.Format("{0} - {1}", CrossGeofence.Id, "Too many geofences.");
                    break;
                case Android.Gms.Location.GeofenceStatusCodes.GeofenceTooManyPendingIntents:
                    message = string.Format("{0} - {1}", CrossGeofence.Id, "Too many pending intents.");
                    break;
                case Android.Gms.Location.GeofenceStatusCodes.GeofenceNotAvailable:
                    message = string.Format("{0} - {1}", CrossGeofence.Id, "Geofence not available.");
                    break;
            }

            System.Diagnostics.Debug.WriteLine(message);

            if (statusCode != Android.Gms.Location.GeofenceStatusCodes.Success && statusCode != Android.Gms.Location.GeofenceStatusCodes.SuccessCache && IsMonitoring)
            {
                StopMonitoring();


                if (!string.IsNullOrEmpty(message))
                    CrossGeofence.GeofenceListener.OnError(message);
            }
        }

        public void OnConnectionSuspended(int cause)
        {
            string message = string.Format("{0} - {1} {2}", CrossGeofence.Id, "Connection to Google Play services suspended with error code ", cause);
            System.Diagnostics.Debug.WriteLine(message);
            CrossGeofence.GeofenceListener.OnError(message);
        }

        GeofenceNotInitializedException NewGeofenceNotInitializedException()
        {
            string description = string.Format("{0} - {1}", CrossGeofence.Id, "Plugin is not initialized. Should initialize before use with CrossGeofence Initialize method. Example:  CrossGeofence.Initialize<CrossGeofenceListener>()");

            return new GeofenceNotInitializedException(description);
        }


      public void StopMonitoring(List<string> regionIdentifiers)
      {
          if (!CrossGeofence.IsInitialized)
          {
              throw NewGeofenceNotInitializedException();
          }

        
          foreach(string identifier in regionIdentifiers)
          {
              RemoveRegion(identifier);
              GeofenceStore.SharedInstance.RemoveGeofenceRegion(identifier);
              CrossGeofence.GeofenceListener.OnMonitoringStopped(identifier);
          }

          Android.Gms.Location.LocationServices.GeofencingApi.RemoveGeofences(mGoogleApiClient, regionIdentifiers).SetResultCallback(this);
 
          if (mRegions.Count == 0)
          {
              
              CrossGeofence.GeofenceListener.OnMonitoringStopped();

              StopLocationUpdates();
             

              mGoogleApiClient.Disconnect();
          }
    
        
      }

      public void StopMonitoring(string regionIdentifier)
      {
          if (!CrossGeofence.IsInitialized)
          {
              throw NewGeofenceNotInitializedException();
          }

          RemoveRegion(regionIdentifier);
          GeofenceStore.SharedInstance.RemoveGeofenceRegion(regionIdentifier);
          Android.Gms.Location.LocationServices.GeofencingApi.RemoveGeofences(mGoogleApiClient, new List<string>() { regionIdentifier }).SetResultCallback(this);
          CrossGeofence.GeofenceListener.OnMonitoringStopped(regionIdentifier);

          if (mRegions.Count == 0)
          {
 
              
              
               StopLocationUpdates();
            

              mGoogleApiClient.Disconnect();

              CrossGeofence.GeofenceListener.OnMonitoringStopped();

              
          }

         
      }
      private void RemoveRegion(string regionIdentifier)
      {
          if (mRegions.ContainsKey(regionIdentifier))
          {
              mRegions.Remove(regionIdentifier);
          }

          if (mGeofenceResults.ContainsKey(regionIdentifier))
          {
              mGeofenceResults.Remove(regionIdentifier);
          }
      }

      protected void StartLocationUpdates()
      {
          Android.Gms.Location.LocationRequest mLocationRequest = new Android.Gms.Location.LocationRequest();
          mLocationRequest.SetInterval(CrossGeofence.LocationUpdatesInterval == 0 ? 30000 : CrossGeofence.LocationUpdatesInterval);
          mLocationRequest.SetFastestInterval(CrossGeofence.FastestLocationUpdatesInterval == 0 ? 5000 : CrossGeofence.FastestLocationUpdatesInterval);
          string priorityType="Balanced Power";
          switch(CrossGeofence.GeofencePriority)
          {
              case GeofencePriority.HighAccuracy:
                  priorityType="High Accuracy";
                  mLocationRequest.SetPriority(Android.Gms.Location.LocationRequest.PriorityHighAccuracy);
                  break;
              case GeofencePriority.LowAccuracy:
                  priorityType="Low Accuracy";
                  mLocationRequest.SetPriority(Android.Gms.Location.LocationRequest.PriorityLowPower);
                  break;
              case GeofencePriority.LowestAccuracy:
                  priorityType="Lowest Accuracy";
                  mLocationRequest.SetPriority(Android.Gms.Location.LocationRequest.PriorityNoPower);
                  break;
              case GeofencePriority.MediumAccuracy:
              case GeofencePriority.AcceptableAccuracy:
              default:
                  mLocationRequest.SetPriority(Android.Gms.Location.LocationRequest.PriorityBalancedPowerAccuracy);
                  break;
          }
        
          System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2}", CrossGeofence.Id, "Priority set to",priorityType));
          //(Regions.Count == 0) ? (CrossGeofence.SmallestDisplacement==0?50 :CrossGeofence.SmallestDisplacement): Regions.Min(s => (float)s.Value.Radius)
          if(CrossGeofence.SmallestDisplacement>0)
          {
              mLocationRequest.SetSmallestDisplacement(CrossGeofence.SmallestDisplacement);
              System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2} meters", CrossGeofence.Id, "Location smallest displacement set to", CrossGeofence.SmallestDisplacement));
          }
     
          Android.Gms.Location.LocationServices.FusedLocationApi.RequestLocationUpdates(mGoogleApiClient, mLocationRequest, GeofenceLocationListener.SharedInstance);
      }
      protected void StopLocationUpdates()
      {
          Android.Gms.Location.LocationServices.FusedLocationApi.RemoveLocationUpdates(mGoogleApiClient, GeofenceLocationListener.SharedInstance);
      }
      

    }
}
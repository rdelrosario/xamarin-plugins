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

namespace Geofence.Plugin
{
  /// <summary>
  /// Implementation for Feature
  /// </summary>
  /// 
  [Service(Exported = false)]
  public class GeofenceImplementation : IntentService,IGeofence, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener, IResultCallback
  {

      public const string GeoReceiverAction="ACTION_RECEIVE_GEOFENCE";
      public const string Tag = "CrossGeofence";
  
      private Dictionary<string,GeofenceCircularRegion> mRegions;

      private Dictionary<string, GeofenceResult> mGeoreferenceResults;

      //private bool isMonitoring;

      private IGoogleApiClient mGoogleApiClient;

      // Defines the allowable request types
	  enum RequestType { Add, Default }
	  RequestType mRequestType;

      public List<GeofenceCircularRegion> Regions { get { return mRegions.Values.ToList(); } }
      public bool IsMonitoring { get { return mRegions.Count>0; } }

      private PendingIntent GeofenceTransitionPendingIntent
      {
          get
          {
              //var intent = new Intent(this, typeof(GeofenceImplementation));
              var intent = new Intent( string.Format("{0}.{1}", Android.App.Application.Context.PackageName,GeoReceiverAction));
              return PendingIntent.GetBroadcast(this, 0, intent, PendingIntentFlags.UpdateCurrent);
          }
      }

      public GeofenceImplementation()
	  {
          InitializeGoogleAPI();
          mRegions = new Dictionary<string, GeofenceCircularRegion>();
          mGeoreferenceResults = new Dictionary<string, GeofenceResult>();
	  }
      /// <summary>
      /// Starts geofence monitoring on specified regions
      /// </summary>
      /// <param name="regions"></param>
      public void StartMonitoring(List<GeofenceCircularRegion> regions)
      {
          //Regions = regions;
          if (IsMonitoring)
          {
              StopMonitoring();
          }

          if (mGoogleApiClient.IsConnected)
          {
              AddGeofences(regions);
          }else
          {
              if (!mGoogleApiClient.IsConnecting)
              {
                  mGoogleApiClient.Connect();

              }
              mRequestType = RequestType.Add;
          }
      }
      private void AddGeofences(List<GeofenceCircularRegion> regions)
      {
          List<Android.Gms.Location.IGeofence> geofenceList = new List<Android.Gms.Location.IGeofence>();
         
          foreach (GeofenceCircularRegion region in regions)
          {
              geofenceList.Add(new Android.Gms.Location.GeofenceBuilder()
                .SetRequestId(region.Tag)
                .SetTransitionTypes(Android.Gms.Location.Geofence.GeofenceTransitionDwell | Android.Gms.Location.Geofence.GeofenceTransitionEnter | Android.Gms.Location.Geofence.GeofenceTransitionExit)
                .SetCircularRegion(region.Latitude, region.Longitude, (float)region.Radius)
                .SetLoiteringDelay(region.MinimumDuration)
                  //.SetExpirationDuration(mExpirationDuration)
                .Build());
              mRegions.Add(region.Tag, region);

          }
          Android.Gms.Location.GeofencingRequest request = new Android.Gms.Location.GeofencingRequest.Builder().SetInitialTrigger(Android.Gms.Location.GeofencingRequest.InitialTriggerEnter).AddGeofences(geofenceList).Build();

          Android.Gms.Location.LocationServices.GeofencingApi.AddGeofences(mGoogleApiClient, request, GeofenceTransitionPendingIntent).SetResultCallback(this);
        
          mRequestType = RequestType.Default;
      }

      protected override void OnHandleIntent(Intent intent)
      {

          Bundle extras = intent.Extras;
          Android.Gms.Location.GeofencingEvent geofencingEvent = Android.Gms.Location.GeofencingEvent.FromIntent(intent);

          if (geofencingEvent.HasError)
          {
              string errorMessage = Android.Gms.Location.GeofenceStatusCodes.GetStatusCodeString(geofencingEvent.ErrorCode);
  
              Log.Error(Tag, errorMessage);
          }
          // Get the transition type.
          int geofenceTransition = geofencingEvent.GeofenceTransition;
          // Get the geofences that were triggered. A single event can trigger
          // multiple geofences.
          IList<Android.Gms.Location.IGeofence> triggeringGeofences = geofencingEvent.TriggeringGeofences;
           // Get the transition details as a String.
           /* String geofenceTransitionDetails = getGeofenceTransitionDetails(
                    this,
                    geofenceTransition,
                    triggeringGeofences
            *    // Send notification and log the transition details.
            sendNotification(geofenceTransitionDetails);
            Log.i(TAG, geofenceTransitionDetails);
                    */
          foreach (Android.Gms.Location.IGeofence geofence in triggeringGeofences)
          {

              if (!mGeoreferenceResults.ContainsKey(geofence.RequestId))
              {
                  mGeoreferenceResults.Add(geofence.RequestId, new GeofenceResult()
                  {
                      Region = mRegions[geofence.RequestId]
                  });
              }

              switch (geofenceTransition)
              {
                  case Android.Gms.Location.Geofence.GeofenceTransitionEnter:

                      mGeoreferenceResults[geofence.RequestId].LastEnterTime = new DateTime();
                      mGeoreferenceResults[geofence.RequestId].LastExitTime = null;
                      CrossGeofence.GeofenceListener.OnRegionEntered(mGeoreferenceResults[geofence.RequestId]);

                      break;
                  case Android.Gms.Location.Geofence.GeofenceTransitionExit:

                      mGeoreferenceResults[geofence.RequestId].LastExitTime = new DateTime();
                      CrossGeofence.GeofenceListener.OnRegionExited(mGeoreferenceResults[geofence.RequestId]);
                  
                      break;
                  case Android.Gms.Location.Geofence.GeofenceTransitionDwell:
                      CrossGeofence.GeofenceListener.OnRegionStay(mGeoreferenceResults[geofence.RequestId]);
                      break;
                  default:
                      // Log.e(TAG, getString(R.string.geofence_transition_invalid_type,
                      // geofenceTransition));
                      break;
              }
          }
          
         

          // Release the wake lock provided by the WakefulBroadcastReceiver.
          GeofenceBroadcastReceiver.CompleteWakefulIntent(intent);
      }
      /// <summary>
      /// Stops geofence monitoring
      /// </summary>
      public void StopMonitoring()
      {
          mRegions.Clear();
          mGeoreferenceResults.Clear();
          Android.Gms.Location.LocationServices.GeofencingApi.RemoveGeofences(mGoogleApiClient,GeofenceTransitionPendingIntent).SetResultCallback(this);
          mGoogleApiClient.Disconnect();
          CrossGeofence.GeofenceListener.OnMonitoringStopped();
      }

      private void InitializeGoogleAPI()
      {
          int queryResult = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(Android.App.Application.Context);

          if (queryResult == ConnectionResult.Success)
          {
              mGoogleApiClient = new GoogleApiClientBuilder(Android.App.Application.Context).AddApi(Android.Gms.Location.LocationServices.Api).AddConnectionCallbacks(this).AddOnConnectionFailedListener(this).Build();
              if (!mGoogleApiClient.IsConnected)
              {
                  mGoogleApiClient.Connect();

              }
              if (Log.IsLoggable("GeofenceService",LogPriority.Debug))
              {
                  Log.Debug("GeofenceService", "Google Play services is available.");
              }
          }
          else
          {
              Log.Error("GeofenceService", "Google Play services is unavailable.");

          }
      }

      public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
      {
         // mInProgress = false;
          // If the error has a resolution, start a Google Play services activity to resolve it
      
          int errorCode = result.ErrorCode;
          Log.Error(Tag, "Connection to Google Play services failed with error code " + errorCode);
      
      }

      public void OnConnected(Bundle connectionHint)
      {
          // Use mRequestType to determine what action to take. Only Add used in this sample
          if (mRequestType == RequestType.Add)
          {
              AddGeofences(mRegions.Values.ToList());
          
          }
      }

      public void OnResult(Java.Lang.Object result)
      {
          
          /* IResult aresult = result.GetObject<IResult>();

           if (result != null)
           {

           }*/
          int statusCode = 0;
          switch (statusCode)
          {
              case Android.Gms.Location.GeofenceStatusCodes.Success:
                  Log.Debug(Tag, "Successfully added Geofence.");
                  break;
              case Android.Gms.Location.GeofenceStatusCodes.Error:
                  Log.Error(Tag, "Error adding Geofence.");
                  break;
              case Android.Gms.Location.GeofenceStatusCodes.GeofenceTooManyGeofences:
                  Log.Error(Tag, "Too many geofences.");
                  break;
              case Android.Gms.Location.GeofenceStatusCodes.GeofenceTooManyPendingIntents:
                  Log.Error(Tag, "Too many pending intents.");
                  break;

          }

          if (statusCode!=Android.Gms.Location.GeofenceStatusCodes.Success && IsMonitoring)
          {
              StopMonitoring();
          }

          

      }
      public void OnConnectionSuspended(int cause)
      {
          Log.Error(Tag, "Connection to Google Play services suspended with error code " + cause);
      }
  }
}
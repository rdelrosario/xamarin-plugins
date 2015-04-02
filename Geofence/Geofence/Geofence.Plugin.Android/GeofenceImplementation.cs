using Geofence.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using Android.Util;
using Android.Gms.Common.Apis;
using Android.Gms.Common;
using Android.App;
using Android.Content;
using Android.OS;


namespace Geofence.Plugin
{
  /// <summary>
  /// Implementation for Feature
  /// </summary>
  /// 
  [Service(Exported = false)]
  public class GeofenceImplementation : IntentService,IGeofence, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener, IResultCallback
  {

      //public List<GeoCircularRegion> Regions { get; private set; }
      private List<Android.Gms.Location.IGeofence> mGeofenceList;

      private IGoogleApiClient mGoogleApiClient;

      // Defines the allowable request types (in this example, we only add geofences)
	  enum RequestType { Add }
	  RequestType mRequestType;

    
    
      PendingIntent GeofenceTransitionPendingIntent
      {
          get
          {
              var intent = new Intent(this, typeof(GeofenceImplementation));
              return PendingIntent.GetService(this, 0, intent, PendingIntentFlags.UpdateCurrent);
          }
      }

      public GeofenceImplementation()
	  {
          InitializeGoogleAPI();
          mGeofenceList = new List<Android.Gms.Location.IGeofence>();
	  }
      /// <summary>
      /// Starts geofence monitoring on specified regions
      /// </summary>
      /// <param name="regions"></param>
      void Start(List<GeoCircularRegion> regions)
      {
          //Regions = regions;
          mGeofenceList.Clear();

          foreach(GeoCircularRegion region in regions)
          {
              mGeofenceList.Add(new Android.Gms.Location.GeofenceBuilder()
				.SetRequestId(region.Tag)
                .SetTransitionTypes(Android.Gms.Location.Geofence.GeofenceTransitionEnter | Android.Gms.Location.Geofence.GeofenceTransitionExit)
				.SetCircularRegion(region.Latitude, region.Longitude, (float)region.Radius)
				//.SetExpirationDuration(mExpirationDuration)
				.Build());
           
          }
         

          if (mGoogleApiClient.IsConnected)
          {
              Android.Gms.Location.GeofencingRequest request = new Android.Gms.Location.GeofencingRequest.Builder().SetInitialTrigger(Android.Gms.Location.GeofencingRequest.InitialTriggerEnter).AddGeofences(mGeofenceList).Build();

              Android.Gms.Location.LocationServices.GeofencingApi.AddGeofences(mGoogleApiClient, request, GeofenceTransitionPendingIntent).SetResultCallback(this);
          }else
          {
              if (!mGoogleApiClient.IsConnecting)
              {
                  mGoogleApiClient.Connect();

              }
              mRequestType = RequestType.Add;
          }
      }
      /// <summary>
      /// Stops geofence monitoring
      /// </summary>
      void Stop()
      {
          Android.Gms.Location.LocationServices.GeofencingApi.RemoveGeofences(mGoogleApiClient,GeofenceTransitionPendingIntent).SetResultCallback(this);
          mGoogleApiClient.Disconnect();
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
         /* mInProgress = false;
          // If the error has a resolution, start a Google Play services activity to resolve it
          if (result.HasResolution)
          {
              try
              {
                  result.StartResolutionForResult(this, Constants.CONNECTION_FAILURE_RESOLUTION_REQUEST);
              }
              catch (Exception ex)
              {
                  Log.Error(Constants.TAG, "Exception while resolving connection error.", ex);
              }
          }
          else
          {
              int errorCode = result.ErrorCode;
              Log.Error(Constants.TAG, "Connection to Google Play services failed with error code " + errorCode);
          }*/
      }

      public void OnConnected(Bundle connectionHint)
      {
          // Use mRequestType to determine what action to take. Only Add used in this sample
          if (mRequestType == RequestType.Add)
          {
              Android.Gms.Location.GeofencingRequest request = new Android.Gms.Location.GeofencingRequest.Builder().SetInitialTrigger(Android.Gms.Location.GeofencingRequest.InitialTriggerEnter).AddGeofences(mGeofenceList).Build();

              Android.Gms.Location.LocationServices.GeofencingApi.AddGeofences(mGoogleApiClient, request, GeofenceTransitionPendingIntent).SetResultCallback(this);
          
          }
      }

      public void OnResult(Java.Lang.Object result)
      {
        
          /* IResult aresult = result.GetObject<IResult>();

           if (result != null)
           {

           }*/

          // Log if adding the geofences was successful
         /* if (Android.Gms.Location.LocationStatusCodes.Success == result.st)
          {
              if (Log.IsLoggable(Constants.TAG, LogPriority.Debug))
              {
                  Log.Debug(Constants.TAG, "Added geofences successfully.");
              }
          }
          else
          {
              Log.Error(Constants.TAG, "Failed to add geofences. Status code: " + statusCode);
          }
          // turn off the in progress flag and disconnect the client
          mInProgress = false;
          mLoactionClient.Disconnect();*/
      }
  }
}
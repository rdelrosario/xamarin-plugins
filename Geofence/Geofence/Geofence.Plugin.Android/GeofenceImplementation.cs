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

      public const string GeoReceiverAction = "ACTION_RECEIVE_GEOFENCE";
  
      private Dictionary<string,GeofenceCircularRegion> mRegions;

      private Dictionary<string, GeofenceResult> mGeoreferenceResults;

      private IGoogleApiClient mGoogleApiClient;

      // Defines the allowable request types
	  enum RequestType { Add, Default }
	  RequestType mRequestType;

      public IEnumerable<GeofenceCircularRegion> Regions { get { return mRegions.Values; } }
      public bool IsMonitoring { get { return mRegions.Count>0; } }

      private PendingIntent GeofenceTransitionPendingIntent
      {
          get
          {

             //var intent = new Intent(Android.App.Application.Context, typeof(GeofenceBroadcastReceiver));
             // intent.SetAction(string.Format("{0}.{1}", Android.App.Application.Context.PackageName, GeoReceiverAction));
              var intent = new Intent(string.Format("{0}.{1}", Android.App.Application.Context.PackageName, GeoReceiverAction));
              return PendingIntent.GetBroadcast(Android.App.Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
          }
      }

      public GeofenceImplementation()
	  {
          mRegions = new Dictionary<string, GeofenceCircularRegion>();
          mGeoreferenceResults = new Dictionary<string, GeofenceResult>();
          mRequestType = RequestType.Default;
          InitializeGoogleAPI();

	  }
      /// <summary>
      /// Starts geofence monitoring on specified regions
      /// </summary>
      /// <param name="regions"></param>
      public void StartMonitoring(IList<GeofenceCircularRegion> regions)
      {
          if (!CrossGeofence.IsInitialized)
          {
              throw NewGeofenceNotInitializedException();
          }
         
          //Check if location services are enabled
          if (!((Android.Locations.LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService)).IsProviderEnabled(Android.Locations.LocationManager.GpsProvider))
          {
              string message = string.Format("{0} - {1}", CrossGeofence.Tag, "You need to enable Location Services");
              System.Diagnostics.Debug.WriteLine(message);
              CrossGeofence.GeofenceListener.OnError(message);
              return;
          }

          //If monitoring stop
          if (IsMonitoring)
          {
              mRegions.Clear();
              mGeoreferenceResults.Clear();
              Android.Gms.Location.LocationServices.GeofencingApi.RemoveGeofences(mGoogleApiClient, GeofenceTransitionPendingIntent).SetResultCallback(this);
        
          }

          foreach (GeofenceCircularRegion region in regions)
          {

              mRegions.Add(region.Tag, region);

              /* new GeofenceCircularRegion()
              {
                  Latitude=region.Latitude,
                  Longitude=region.Longitude,
                  Tag=region.Tag,
                  Radius=region.Radius,
                  MinimumDuration=region.MinimumDuration
              }*/

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
                mRequestType = RequestType.Add;

            }
        }

        private void AddGeofences()
        {
            try
            {
                List<Android.Gms.Location.IGeofence> geofenceList = new List<Android.Gms.Location.IGeofence>();

                foreach (GeofenceCircularRegion region in Regions)
                {
                    geofenceList.Add(new Android.Gms.Location.GeofenceBuilder()
                    .SetRequestId(region.Tag)
                    .SetTransitionTypes(Android.Gms.Location.Geofence.GeofenceTransitionDwell | Android.Gms.Location.Geofence.GeofenceTransitionEnter | Android.Gms.Location.Geofence.GeofenceTransitionExit)
                    .SetCircularRegion(region.Latitude, region.Longitude, (float)region.Radius)
                    .SetLoiteringDelay(region.MinimumDuration)
                        //.SetNotificationResponsiveness(mNotificationResponsivness)
                    .SetExpirationDuration(Android.Gms.Location.Geofence.NeverExpire)
                    .Build());

                }
                Android.Gms.Location.GeofencingRequest request = new Android.Gms.Location.GeofencingRequest.Builder().SetInitialTrigger(Android.Gms.Location.GeofencingRequest.InitialTriggerEnter).AddGeofences(geofenceList).Build();

                Android.Gms.Location.LocationServices.GeofencingApi.AddGeofences(mGoogleApiClient, request, GeofenceTransitionPendingIntent).SetResultCallback(this);

                mRequestType = RequestType.Default;

            }catch(Java.Lang.Exception ex1)
            {
                string message = string.Format("{0} - Error: {1}", CrossGeofence.Tag, ex1.ToString());
                System.Diagnostics.Debug.WriteLine(message);
                CrossGeofence.GeofenceListener.OnError(message);
            }
            catch (System.Exception ex2)
            {
                string message = string.Format("{0} - Error: {1}", CrossGeofence.Tag, ex2.ToString());
                System.Diagnostics.Debug.WriteLine(message);
                CrossGeofence.GeofenceListener.OnError(message);
            }
            
        }

        protected override void OnHandleIntent(Intent intent)
        {

            Bundle extras = intent.Extras;
            Android.Gms.Location.GeofencingEvent geofencingEvent = Android.Gms.Location.GeofencingEvent.FromIntent(intent);

            if (geofencingEvent.HasError)
            {
                string errorMessage = Android.Gms.Location.GeofenceStatusCodes.GetStatusCodeString(geofencingEvent.ErrorCode);
                string message = string.Format("{0} - {1}", CrossGeofence.Tag, errorMessage);
                System.Diagnostics.Debug.WriteLine(message);
                CrossGeofence.GeofenceListener.OnError(message);
            }
            // Get the transition type.
            int geofenceTransition = geofencingEvent.GeofenceTransition;
            // Get the geofences that were triggered. A single event can trigger
            // multiple geofences.
            IList<Android.Gms.Location.IGeofence> triggeringGeofences = geofencingEvent.TriggeringGeofences;
            // Get the transition details as a String.
        
            List<string> geofenceIds = new List<string>();
            GeofenceTransition gTransition = GeofenceTransition.Unknown;

            foreach (Android.Gms.Location.IGeofence geofence in triggeringGeofences)
            {

                if (!mGeoreferenceResults.ContainsKey(geofence.RequestId))
                {
                    System.Diagnostics.Debug.WriteLine("Requested Key:"+geofence.RequestId);
                    System.Diagnostics.Debug.WriteLine("Regions Count:",mRegions.Count);
                    System.Diagnostics.Debug.WriteLine("Contained Keys:");
                    foreach (var r in mRegions.Keys)
                    {
                        System.Diagnostics.Debug.WriteLine(r);
                    }
                    mGeoreferenceResults.Add(geofence.RequestId, new GeofenceResult()
                        {
                            Region = mRegions[geofence.RequestId]
                        });
               
                }
               
                switch (geofenceTransition)
                {
                    case Android.Gms.Location.Geofence.GeofenceTransitionEnter:
                        gTransition = GeofenceTransition.Entered;
                        mGeoreferenceResults[geofence.RequestId].LastEnterTime = DateTime.UtcNow;
                        mGeoreferenceResults[geofence.RequestId].LastExitTime = null;
                        mGeoreferenceResults[geofence.RequestId].Transition = GeofenceTransition.Entered;
                        CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeoreferenceResults[geofence.RequestId]);
                      
                        break;
                    case Android.Gms.Location.Geofence.GeofenceTransitionExit:
                        gTransition = GeofenceTransition.Exited;
                        mGeoreferenceResults[geofence.RequestId].LastExitTime = DateTime.UtcNow;
                        mGeoreferenceResults[geofence.RequestId].Transition = GeofenceTransition.Exited;
                        CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeoreferenceResults[geofence.RequestId]);
                      
                        break;
                    case Android.Gms.Location.Geofence.GeofenceTransitionDwell:
                        gTransition = GeofenceTransition.Stayed;
                        mGeoreferenceResults[geofence.RequestId].Transition = GeofenceTransition.Stayed;
                        CrossGeofence.GeofenceListener.OnRegionStateChanged(mGeoreferenceResults[geofence.RequestId]);
                    
                        break;
                    default:
                        string message = string.Format("{0} - {1}", CrossGeofence.Tag, "Invalid transition type");
                        System.Diagnostics.Debug.WriteLine(message);
                     // CrossGeofence.GeofenceListener.OnError(message);
                        break;
                }

                geofenceIds.Add(geofence.RequestId);
        
              
          }

          if (CrossGeofence.EnableNotification)
          {
              try
              {
                  Context context = Android.App.Application.Context;

                  CreateNotification(string.Join(", ", geofenceIds), string.Format("{0} {1}", GeofenceResult.GetTransitionString(gTransition), "geofence(s) region."));

              }
              catch (Java.Lang.Exception ex)
              {
                  System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Tag, ex.ToString()));
              }
              catch (System.Exception ex1)
              {
                  System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Tag, ex1.ToString()));
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
            if (!CrossGeofence.IsInitialized)
            {
                throw NewGeofenceNotInitializedException();
            }

            mRegions.Clear();
            mGeoreferenceResults.Clear();
            Android.Gms.Location.LocationServices.GeofencingApi.RemoveGeofences(mGoogleApiClient, GeofenceTransitionPendingIntent).SetResultCallback(this);
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

                string message = string.Format("{0} - {1}", CrossGeofence.Tag, "Google Play services is available.");
                System.Diagnostics.Debug.WriteLine(message);

            }
            else
            {
            
                string message = string.Format("{0} - {1}", CrossGeofence.Tag, "Google Play services is unavailable.");
                System.Diagnostics.Debug.WriteLine(message);
                CrossGeofence.GeofenceListener.OnError(message);
            }
        }

        public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
        {
            int errorCode = result.ErrorCode;
            string message = string.Format("{0} - {1} {2}", CrossGeofence.Tag, "Connection to Google Play services failed with error code ", errorCode);
            System.Diagnostics.Debug.WriteLine(message);
            CrossGeofence.GeofenceListener.OnError(message);
      
        }

        public void OnConnected(Bundle connectionHint)
        {
            // Use mRequestType to determine what action to take. Only Add used in this sample
            if (mRequestType == RequestType.Add)
            {
                AddGeofences();
          
            }
        }

        public void OnResult(Java.Lang.Object result)
        {
            var res = result.JavaCast<IResult>();

            int statusCode = res.Status.StatusCode;
            string message = string.Empty;

            switch (res.Status.StatusCode)
            {
                case Android.Gms.Location.GeofenceStatusCodes.Success:
                    message = string.Format("{0} - {1}", CrossGeofence.Tag, "Successfully added Geofence.");
                    CrossGeofence.GeofenceListener.OnMonitoringStarted();
                    break;
                case Android.Gms.Location.GeofenceStatusCodes.Error:
                    message = string.Format("{0} - {1}", CrossGeofence.Tag, "Error adding Geofence.");
                    break;
                case Android.Gms.Location.GeofenceStatusCodes.GeofenceTooManyGeofences:
                    message = string.Format("{0} - {1}", CrossGeofence.Tag, "Too many geofences.");
                    break;
                case Android.Gms.Location.GeofenceStatusCodes.GeofenceTooManyPendingIntents:
                    message = string.Format("{0} - {1}", CrossGeofence.Tag, "Too many pending intents.");
                    break;
                case Android.Gms.Location.GeofenceStatusCodes.GeofenceNotAvailable:
                    message = string.Format("{0} - {1}", CrossGeofence.Tag, "Geofence not available.");
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
            string message = string.Format("{0} - {1} {2}", CrossGeofence.Tag, "Connection to Google Play services suspended with error code ", cause);
            System.Diagnostics.Debug.WriteLine(message);
            CrossGeofence.GeofenceListener.OnError(message);
        }

        GeofenceNotInitializedException NewGeofenceNotInitializedException()
        {
            string description = string.Format("{0} - {1}", CrossGeofence.Tag, "Plugin is not initialized. Should initialize before use with CrossGeofence Initialize method. Example:  CrossGeofence.Initialize<CrossGeofenceListener>()");

            return new GeofenceNotInitializedException(description);
        }

        public static void CreateNotification(string title, string message)
        {

            NotificationCompat.Builder builder = null;
            Context context = Android.App.Application.Context;

            if (CrossGeofence.SoundUri == null)
            {
                CrossGeofence.SoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            }
            try
            {

                if (CrossGeofence.IconResource == 0)
                {
                    CrossGeofence.IconResource = context.ApplicationInfo.Icon;
                }
                else
                {
                    string name = context.Resources.GetResourceName(CrossGeofence.IconResource);

                    if (name == null)
                    {
                        CrossGeofence.IconResource = context.ApplicationInfo.Icon;

                    }
                }

            }
            catch (Android.Content.Res.Resources.NotFoundException ex)
            {
                CrossGeofence.IconResource = context.ApplicationInfo.Icon;
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }



            Intent resultIntent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);


            // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
            const int pendingIntentId = 0;
            PendingIntent resultPendingIntent = PendingIntent.GetActivity(context, pendingIntentId, resultIntent, PendingIntentFlags.OneShot);

            // Build the notification
            builder = new NotificationCompat.Builder(context)
                    .SetAutoCancel(true) // dismiss the notification from the notification area when the user clicks on it
                    .SetContentIntent(resultPendingIntent) // start up this activity when the user clicks the intent.
                    .SetContentTitle(title) // Set the title
                    .SetSound(CrossGeofence.SoundUri)
                    .SetSmallIcon(CrossGeofence.IconResource) // This is the icon to display
                    .SetContentText(message); // the message to display.

            NotificationManager notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            notificationManager.Notify(0, builder.Build());
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
              CrossGeofence.GeofenceListener.OnMonitoringStopped(identifier);
          }

          Android.Gms.Location.LocationServices.GeofencingApi.RemoveGeofences(mGoogleApiClient, regionIdentifiers).SetResultCallback(this);
 
          if (mRegions.Count == 0)
          {
              mGoogleApiClient.Disconnect();
              CrossGeofence.GeofenceListener.OnMonitoringStopped();
          }

        
      }

      public void StopMonitoring(string regionIdentifier)
      {
          if (!CrossGeofence.IsInitialized)
          {
              throw NewGeofenceNotInitializedException();
          }

          RemoveRegion(regionIdentifier);
          Android.Gms.Location.LocationServices.GeofencingApi.RemoveGeofences(mGoogleApiClient, new List<string>() { regionIdentifier }).SetResultCallback(this);
          CrossGeofence.GeofenceListener.OnMonitoringStopped(regionIdentifier);

          if (mRegions.Count == 0)
          {
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

          if (mGeoreferenceResults.ContainsKey(regionIdentifier))
          {
              mGeoreferenceResults.Remove(regionIdentifier);
          }
      }

    }
}
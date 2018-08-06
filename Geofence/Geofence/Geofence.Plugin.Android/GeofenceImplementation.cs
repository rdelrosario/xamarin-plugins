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
using Android.Gms.Location;
using Android;
using Android.Content.PM;
using Android.Gms.Tasks;

namespace Geofence.Plugin
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    /// 
    public class GeofenceImplementation : Java.Lang.Object, Geofence.Plugin.Abstractions.IGeofence,  IResultCallback, Android.Gms.Tasks.IOnCompleteListener, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {

        public int REQUEST_PERMISSIONS_REQUEST_CODE = 101;

        private Dictionary<string,GeofenceCircularRegion> mRegions = GeofenceStore.SharedInstance.GetAll();

        private Dictionary<string, GeofenceResult> mGeofenceResults = new Dictionary<string, GeofenceResult>();

        private GeofenceLocation mLastKnownGeofenceLocation;

        private PendingIntent mGeofencePendingIntent = null;

        private GoogleApiClient mGoogleApiClient;
        
        private GeofencingClient mGeofencingClient;

        private IList<Android.Gms.Location.IGeofence> mGeofenceList;

        private LocationRequest mLocationRequest;

        private FusedLocationProviderClient mFusedLocationProviderClient;

        // Defines the allowable request types
        internal enum RequestType
        {
            Add,
            Update,
            Delete,
            Clear,
            Default

        }

        /// <summary>
        /// Get all regions been monitored
        /// </summary>
        public IReadOnlyDictionary<string, GeofenceCircularRegion> Regions { get { return mRegions; } }

        /// <summary>
        /// Get geofence state change results.
        /// </summary>
        public IReadOnlyDictionary<string, GeofenceResult> GeofenceResults { get { return mGeofenceResults; } }

        private IList<string> mRequestedRegionIdentifiers;

        /// <summary>
        /// Get last known location
        /// </summary>
        public GeofenceLocation LastKnownLocation { get { return mLastKnownGeofenceLocation; } }

        internal RequestType CurrentRequestType { get; set; }

        /// <summary>
        /// Checks if region are been monitored
        /// </summary>
        public bool IsMonitoring { get { return mRegions.Count > 0; } }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Geofence.Plugin.GeofenceImplementation"/> location has error.
        /// </summary>
        /// <value><c>true</c> if location has error; otherwise, <c>false</c>.</value>
        public bool LocationHasError { get; set; }
     
        public bool RequestNotificationPermission { get; set; }
        public bool RequestLocationPermission { get; set; }

        public static object Lock { get; private set; } = new object();

        //IsMonitoring?RequestType.Add:

        private PendingIntent GeofenceTransitionPendingIntent
        {

            // New geofence intent for Android O: 
            // https://stackoverflow.com/questions/50564251/geofencing-doesnt-work-with-jonintentservice-anymore
            // https://developer.android.com/training/location/geofencing
            get
            {
                if (mGeofencePendingIntent != null)
                {
                    return mGeofencePendingIntent;
                }
                var intent = new Intent(Application.Context, typeof(GeofenceTransitionsIntentService));
                System.Diagnostics.Debug.WriteLine("Returning intent");
                mGeofencePendingIntent = PendingIntent.GetService(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
                return mGeofencePendingIntent;
            }
        }

        /// <summary>
        /// Android Geofence plugin implementation
        /// </summary>
        public GeofenceImplementation()
        {
            if (!CheckPermissions())
            {
                RequestPermissions();
            }
            //Check if location services are enabled
            IsLocationEnabled((bool locationIsEnabled) => {
                if(locationIsEnabled)
                {
                    CurrentRequestType = RequestType.Default;
                    if(IsMonitoring)
                    {
                        StartMonitoring(Regions.Values.ToList());
                        System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, "Monitoring was restored"));
                    }
                }
                else
                {
                    string message = string.Format("{0} - {1}", CrossGeofence.Id, "You need to enabled Location Services");
                    System.Diagnostics.Debug.WriteLine(message);
                    CrossGeofence.GeofenceListener.OnError(message);
                }
            });
        }

        public void IsLocationEnabled(Action<bool> returnAction)
        {
            InitializeGoogleAPI();
            if(mGoogleApiClient == null || CheckPermissions() == false)
            {
                returnAction(false);
                return;
            }
            mFusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(Android.App.Application.Context);
            mGeofencingClient = LocationServices.GetGeofencingClient(Application.Context);
            mGeofenceList = new List<Android.Gms.Location.IGeofence>();

            var locationRequestPriority = LocationRequest.PriorityBalancedPowerAccuracy;
            switch (CrossGeofence.GeofencePriority)
            {
                case GeofencePriority.HighAccuracy:
                    locationRequestPriority = LocationRequest.PriorityHighAccuracy;
                    break;
                case GeofencePriority.LowAccuracy:
                    locationRequestPriority = LocationRequest.PriorityLowPower;
                    break;
                case GeofencePriority.LowestAccuracy:
                    locationRequestPriority = LocationRequest.PriorityNoPower;
                    break;
            }
            
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetPriority(locationRequestPriority);
            mLocationRequest.SetInterval(CrossGeofence.LocationUpdatesInterval);
            mLocationRequest.SetFastestInterval(CrossGeofence.FastestLocationUpdatesInterval);

            LocationSettingsRequest.Builder builder = new LocationSettingsRequest.Builder().AddLocationRequest(mLocationRequest);
            var pendingResult = LocationServices.SettingsApi.CheckLocationSettings(mGoogleApiClient, builder.Build());
            pendingResult.SetResultCallback((LocationSettingsResult locationSettingsResult) => {
                if (locationSettingsResult != null)
                {
                    returnAction(locationSettingsResult.Status.StatusCode <= CommonStatusCodes.Success);
                } 
            });
            System.Diagnostics.Debug.WriteLine("End of IsLocationEnabled, clients should be created");
        }

        /// <summary>
        /// Starts monitoring specified region
        /// </summary>
        /// <param name="region"></param>
        public void StartMonitoring(GeofenceCircularRegion region)
        {
            lock (Lock)
            {
                if (IsMonitoring)
                {
                    mGeofencingClient.RemoveGeofences(GeofenceTransitionPendingIntent).AddOnCompleteListener(this);
                }

                if (!mRegions.ContainsKey(region.Id))
                {
                    mRegions.Add(region.Id, region);
                }
            }
            RequestMonitoringStart();
        }

        void RequestMonitoringStart()
        {
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

        /// <summary>
        /// Starts geofence monitoring on specified regions
        /// </summary>
        /// <param name="regions"></param>
        public void StartMonitoring(IList<GeofenceCircularRegion> regions)
        {
            lock (Lock)
            {
              if (IsMonitoring)
              {
                    mGeofencingClient.RemoveGeofences(GeofenceTransitionPendingIntent).AddOnCompleteListener(this);
                }

                foreach (var region in regions)
                {
                    if (!mRegions.ContainsKey(region.Id))
                    {
                        mRegions.Add(region.Id, region);
                    }
                }

                RequestMonitoringStart();
            }
            System.Diagnostics.Debug.WriteLine("Monitoring has begun");
        }

      
        internal void AddGeofenceResult(string identifier)
        {
            mGeofenceResults.Add(identifier, new GeofenceResult()
                {
                    RegionId = identifier,
                    Transition = GeofenceTransition.Unknown
                });
        }

        /// <summary>
        /// Adds geofences using Android's geofence client.
        /// </summary>
        public void AddGeofences()
        {
            try
            {
                // Operate on copy of regions to reduce scope of lock
                var regionsClone = new List<GeofenceCircularRegion>();
                
                lock (Lock)
                    foreach (var region in Regions.Values)
                    {
                        System.Diagnostics.Debug.WriteLine("Here is region " + region.Id);
                        regionsClone.Add(new GeofenceCircularRegion(region));
                        if (GeofenceStore.SharedInstance.Get(region.Id) == null)
                            GeofenceStore.SharedInstance.Save(region);
                    }

                mGeofenceList.Clear();
                foreach (GeofenceCircularRegion region in regionsClone)
                {
                    int transitionTypes = 0;
                    if (region.NotifyOnStay)
                    {
                        transitionTypes |= Android.Gms.Location.Geofence.GeofenceTransitionDwell;
                    }

                    if (region.NotifyOnEntry)
                    {
                        transitionTypes |= Android.Gms.Location.Geofence.GeofenceTransitionEnter;
                    }

                    if (region.NotifyOnExit)
                    {
                        transitionTypes |= Android.Gms.Location.Geofence.GeofenceTransitionExit;
                    }

                    if (transitionTypes != 0)
                    {
                        mGeofenceList.Add(new Android.Gms.Location.GeofenceBuilder()
                        .SetRequestId(region.Id)
                        .SetCircularRegion(region.Latitude, region.Longitude, (float)region.Radius)
                        .SetLoiteringDelay((int)region.StayedInThresholdDuration.TotalMilliseconds)
                        //.SetNotificationResponsiveness(mNotificationResponsivness)
                        .SetExpirationDuration(Android.Gms.Location.Geofence.NeverExpire)
                        .SetTransitionTypes(transitionTypes)
                        .Build());
                        CrossGeofence.GeofenceListener.OnMonitoringStarted(region.Id);
                    }
                }

                System.Diagnostics.Debug.WriteLine("Add geofences");
                if (mGeofenceList.Count > 0)
                {
                    mGeofencingClient.AddGeofences(GetGeofencingRequest(mGeofenceList), GeofenceTransitionPendingIntent).AddOnCompleteListener(this);
                    CurrentRequestType = RequestType.Default;
                }
            }
            catch (Java.Lang.Exception ex1)
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

        private GeofencingRequest GetGeofencingRequest(IList<Android.Gms.Location.IGeofence> geofenceList)
        {            
            GeofencingRequest.Builder builder = new GeofencingRequest.Builder();
            builder.SetInitialTrigger(GeofencingRequest.InitialTriggerEnter);
            builder.AddGeofences(geofenceList);
            System.Diagnostics.Debug.WriteLine("Request");
            return builder.Build();
        }

        /// <summary>
        /// Stops monitoring a specific geofence region
        /// </summary>
        /// <param name="regionIdentifier"></param>
        public void StopMonitoring(string regionIdentifier)
        {

            StopMonitoring(new List<string>() { regionIdentifier });
        }

        internal void OnMonitoringRemoval()
        {
            if (mRegions.Count == 0)
            {

                CrossGeofence.GeofenceListener.OnMonitoringStopped();

                StopLocationUpdates();


                mGoogleApiClient.Disconnect();
            }
        }

        private void RemoveGeofences(IList<string> regionIdentifiers)
        {           
            foreach (string identifier in regionIdentifiers)
            {
                //Remove this region from regions dictionary and results
                RemoveRegion(identifier);
                //Remove from persistent store
                GeofenceStore.SharedInstance.Remove(identifier);
                //Notify monitoring was stopped
                CrossGeofence.GeofenceListener.OnMonitoringStopped(identifier);
            }

            //Stop Monitoring
            if (regionIdentifiers.Count > 0)
                mGeofencingClient.RemoveGeofences(regionIdentifiers).AddOnCompleteListener(this);

            //Check if there are still regions
            OnMonitoringRemoval();
            System.Diagnostics.Debug.WriteLine("Removed specific regions");
        }

        /// <summary>
        /// Stops monitoring specified geofence regions
        /// </summary>
        public void StopMonitoring(IList<string> regionIdentifiers)
        {
            mRequestedRegionIdentifiers = new List<string>();
            ((List<string>)mRequestedRegionIdentifiers).AddRange(regionIdentifiers);

            if (IsMonitoring)
            {
                RemoveGeofences(mRequestedRegionIdentifiers);
            }
            else
            {
                //If not connection then connect
                if (!mGoogleApiClient.IsConnecting)
                {
                    mGoogleApiClient.Connect();
                }
                //Request to add geofence regions once connected
                CurrentRequestType = RequestType.Delete;
            }
            //
        }

        /// <summary>
        /// Stops monitoring all geofence regions
        /// </summary>
        public void StopMonitoringAllRegions()
        {
            if (IsMonitoring && mGoogleApiClient.IsConnected)
            {
                RemoveGeofences();
            }
            else
            {
                //If not connection then connect
                if (!mGoogleApiClient.IsConnecting)
                {
                    mGoogleApiClient.Connect();

                }
                //Request to add geofence regions once connected
                CurrentRequestType = RequestType.Clear;
            }


        }

        private void RemoveGeofences()
        {            
            lock (Lock)
            {
                GeofenceStore.SharedInstance.RemoveAll();
                mRegions.Clear();
                mGeofenceResults.Clear();
                StopLocationUpdates();
                IList<string> regionIds = mRegions.Select(r => r.Key).ToList();
                if (regionIds.Count > 0)
                    mGeofencingClient.RemoveGeofences(regionIds).AddOnCompleteListener(this);
                mGoogleApiClient.Disconnect();
            }
            System.Diagnostics.Debug.WriteLine("Cleared the geofences");
            CrossGeofence.GeofenceListener.OnMonitoringStopped();
        }

        private void InitializeGoogleAPI()
        {
            int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Android.App.Application.Context);

            if (queryResult == ConnectionResult.Success)
            {
                if (mGoogleApiClient == null)
                {
                    mGoogleApiClient = new GoogleApiClient.Builder(Android.App.Application.Context).AddApi(Android.Gms.Location.LocationServices.API).AddConnectionCallbacks(this).AddOnConnectionFailedListener(this).Build();
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

        /// <summary>
        /// Google play connection failed handling
        /// </summary>
        /// <param name="result"></param>
        public void OnConnectionFailed(Android.Gms.Common.ConnectionResult result)
        {
            int errorCode = result.ErrorCode;
            string message = string.Format("{0} - {1} {2}", CrossGeofence.Id, "Connection to Google Play services failed with error code ", errorCode);
            System.Diagnostics.Debug.WriteLine(message);
            CrossGeofence.GeofenceListener.OnError(message);
      
        }

        internal void SetLastKnownLocation(Android.Locations.Location location)
        {
            if (location != null)
            {
                GeofenceLocation eventLoc;
                lock (Lock)
                {
                    if (mLastKnownGeofenceLocation == null)
                    {
                        mLastKnownGeofenceLocation = new GeofenceLocation();
                    }
                    mLastKnownGeofenceLocation.Latitude = location.Latitude;
                    mLastKnownGeofenceLocation.Longitude = location.Longitude;
                    double seconds = location.Time / 1000;
                    mLastKnownGeofenceLocation.Date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local).AddSeconds(seconds);
                    mLastKnownGeofenceLocation.Accuracy = location.Accuracy;
                    eventLoc = new GeofenceLocation(mLastKnownGeofenceLocation);
                }
                CrossGeofence.GeofenceListener.OnLocationChanged(eventLoc);
            }
            System.Diagnostics.Debug.WriteLine("Setting last location");
        }

        /// <summary>
        /// On Google play services Connection handling
        /// </summary>
        /// <param name="connectionHint"></param>
 
        async public void OnConnected(Bundle connectionHint)
        {
            Android.Locations.Location location = await mFusedLocationProviderClient.GetLastLocationAsync();
            SetLastKnownLocation(location);
            if (CurrentRequestType == RequestType.Add)
            {
                AddGeofences();
                StartLocationUpdates();
            }
            else if (CurrentRequestType == RequestType.Clear)
            {
                RemoveGeofences();
            }
            else if (CurrentRequestType == RequestType.Delete)
            {
                if (mRequestedRegionIdentifiers != null)
                {
                    RemoveGeofences(mRequestedRegionIdentifiers);
                }
            }

            CurrentRequestType = RequestType.Default;
            System.Diagnostics.Debug.WriteLine("Connection obtained, starting service");
        }

        /// <summary>
        /// On Geofence Request Result
        /// </summary>
        /// <param name="result"></param>
        public void OnResult(Java.Lang.Object result)
        {
            var res = result.JavaCast<IResult>();
          
            int statusCode = res.Status.StatusCode;
            string message = string.Empty;

            switch (res.Status.StatusCode)
            {
                case Android.Gms.Location.GeofenceStatusCodes.SuccessCache:
                case Android.Gms.Location.GeofenceStatusCodes.Success:
                    if (CurrentRequestType == RequestType.Add)
                    {
                        message = string.Format("{0} - {1}", CrossGeofence.Id, "Successfully added Geofence.");
                        
                        foreach (GeofenceCircularRegion region in Regions.Values)
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
                // Rather than force killing all running geofences, delegate action on geofence failures to the application.
                // This lets the application decide to ignore the error, perform retry logic, stop monitoring as below, or any other behavior.
                // StopMonitoringAllRegions();
                ((GeofenceImplementation)CrossGeofence.Current).LocationHasError = true;

                if (!string.IsNullOrEmpty(message))
                    CrossGeofence.GeofenceListener.OnError(message);
            }
        }

        /// <summary>
        /// Connection suspended handling
        /// </summary>
        /// <param name="cause"></param>
        public void OnConnectionSuspended(int cause)
        {
            string message = string.Format("{0} - {1} {2}", CrossGeofence.Id, "Connection to Google Play services suspended with error code ", cause);
            System.Diagnostics.Debug.WriteLine(message);
            CrossGeofence.GeofenceListener.OnError(message);
        }

        private void RemoveRegion(string regionIdentifier)
        {
            lock (Lock)
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
            System.Diagnostics.Debug.WriteLine("Removed a region");
        }

        internal void StartLocationUpdates()
        {
            Android.Gms.Location.LocationRequest mLocationRequest = new Android.Gms.Location.LocationRequest();
            mLocationRequest.SetInterval(CrossGeofence.LocationUpdatesInterval == 0 ? 30000 : CrossGeofence.LocationUpdatesInterval);
            mLocationRequest.SetFastestInterval(CrossGeofence.FastestLocationUpdatesInterval == 0 ? 5000 : CrossGeofence.FastestLocationUpdatesInterval);
            string priorityType = "Balanced Power";
            switch (CrossGeofence.GeofencePriority)
            {
                case GeofencePriority.HighAccuracy:
                    priorityType = "High Accuracy";
                    mLocationRequest.SetPriority(Android.Gms.Location.LocationRequest.PriorityHighAccuracy);
                    break;
                case GeofencePriority.LowAccuracy:
                    priorityType = "Low Accuracy";
                    mLocationRequest.SetPriority(Android.Gms.Location.LocationRequest.PriorityLowPower);
                    break;
                case GeofencePriority.LowestAccuracy:
                    priorityType = "Lowest Accuracy";
                    mLocationRequest.SetPriority(Android.Gms.Location.LocationRequest.PriorityNoPower);
                    break;
                case GeofencePriority.MediumAccuracy:
                case GeofencePriority.AcceptableAccuracy:
                default:
                    mLocationRequest.SetPriority(Android.Gms.Location.LocationRequest.PriorityBalancedPowerAccuracy);
                    break;
            }
        
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2}", CrossGeofence.Id, "Priority set to", priorityType));
            //(Regions.Count == 0) ? (CrossGeofence.SmallestDisplacement==0?50 :CrossGeofence.SmallestDisplacement): Regions.Min(s => (float)s.Value.Radius)
            if (CrossGeofence.SmallestDisplacement > 0)
            {
                mLocationRequest.SetSmallestDisplacement(CrossGeofence.SmallestDisplacement);
                System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}: {2} meters", CrossGeofence.Id, "Location smallest displacement set to", CrossGeofence.SmallestDisplacement));
            }
            
            try 
            {
                mFusedLocationProviderClient.RequestLocationUpdatesAsync(this.mLocationRequest, GeofenceLocationListener.SharedInstance);
            }
            catch(System.Exception e)
            {
                // Do not crash the app if permissions are disabled on Android Marshmallow
                System.Diagnostics.Debug.WriteLine(e.Message);
                CrossGeofence.GeofenceListener.OnError(e.Message);
            }
        }

        internal void StopLocationUpdates()
        {
            mFusedLocationProviderClient.RemoveLocationUpdates(GeofenceLocationListener.SharedInstance);
        }

        private bool CheckPermissions()
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                var permissionState = Application.Context.CheckSelfPermission(Manifest.Permission.AccessFineLocation);
                System.Diagnostics.Debug.WriteLine("Checking permissions");
                return permissionState == (int)Permission.Granted;
            }
            else
                return true;
        }

        private void RequestPermissions()
        {            
            ActivityCompat.RequestPermissions((Activity) Application.Context, new[] { Manifest.Permission.AccessFineLocation }, REQUEST_PERMISSIONS_REQUEST_CODE);
            System.Diagnostics.Debug.WriteLine("Requesting permissions");
        }

        public void OnComplete(Task task)
        {
            CurrentRequestType = RequestType.Default;
            System.Diagnostics.Debug.WriteLine("Completed request");
        }

    }
}
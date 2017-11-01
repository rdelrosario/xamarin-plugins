using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Support.V4.App;
using Geofence.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin
{
    /// <summary>
    /// GeofenceTransitionsIntentService class
    /// Service that handles geofence events
    /// </summary>
    [Service]
    public class GeofenceTransitionsIntentService : IntentService
    {
        static int NotificationId = 0;
        const int  NotificationMaxId = 6;
        /// <summary>
        /// Handles geofence intent arrival
        /// </summary>
        /// <param name="intent"></param>
        protected override void OnHandleIntent(Intent intent)
        {
            try 
			{
            	Context context = Android.App.Application.Context;
            	Bundle extras = intent.Extras;
            	Android.Gms.Location.GeofencingEvent geofencingEvent = Android.Gms.Location.GeofencingEvent.FromIntent(intent);
            	
            	if (geofencingEvent.HasError)
            	{
            	    string errorMessage = Android.Gms.Location.GeofenceStatusCodes.GetStatusCodeString(geofencingEvent.ErrorCode);
            	    string message = string.Format("{0} - {1}", CrossGeofence.Id, errorMessage);
            	    System.Diagnostics.Debug.WriteLine(message);
            	    ((GeofenceImplementation)CrossGeofence.Current).LocationHasError = true;
            	    CrossGeofence.GeofenceListener.OnError(message);
            	    
            	    return;
            	}
            	
            	// Get the transition type.
            	int geofenceTransition = geofencingEvent.GeofenceTransition;
            	
            	// Get the geofences that were triggered. A single event can trigger multiple geofences.
            	IList<Android.Gms.Location.IGeofence> triggeringGeofences = geofencingEvent.TriggeringGeofences;
            	
            	GeofenceTransition gTransition = GeofenceTransition.Unknown;
            	
            	((GeofenceImplementation)CrossGeofence.Current).CurrentRequestType = Geofence.Plugin.GeofenceImplementation.RequestType.Update;
            	
            	foreach (Android.Gms.Location.IGeofence geofence in triggeringGeofences)
            	{
            	
            	    if (!CrossGeofence.Current.GeofenceResults.ContainsKey(geofence.RequestId))
            	    {
            	        ((GeofenceImplementation)CrossGeofence.Current).AddGeofenceResult(geofence.RequestId);
            	
            	    }
            	    //geofencingEvent.TriggeringLocation.Accuracy
            	    CrossGeofence.Current.GeofenceResults[geofence.RequestId].Latitude = geofencingEvent.TriggeringLocation.Latitude;
            	    CrossGeofence.Current.GeofenceResults[geofence.RequestId].Longitude = geofencingEvent.TriggeringLocation.Longitude;
            	    CrossGeofence.Current.GeofenceResults[geofence.RequestId].Accuracy = geofencingEvent.TriggeringLocation.Accuracy;
            	
            	    double seconds = geofencingEvent.TriggeringLocation.Time / 1000;
            	    DateTime resultDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(seconds).ToLocalTime();
            	
            	    //DateTime resultDate = DateTime.Now;
            	
            	    switch (geofenceTransition)
            	    {
            	        case Android.Gms.Location.Geofence.GeofenceTransitionEnter:
            	            gTransition = GeofenceTransition.Entered;
            	            CrossGeofence.Current.GeofenceResults[geofence.RequestId].LastEnterTime = resultDate;
            	            CrossGeofence.Current.GeofenceResults[geofence.RequestId].LastExitTime = null;
            	            break;
            	        case Android.Gms.Location.Geofence.GeofenceTransitionExit:
            	            gTransition = GeofenceTransition.Exited;
            	            CrossGeofence.Current.GeofenceResults[geofence.RequestId].LastExitTime = resultDate;
            	            break;
            	        case Android.Gms.Location.Geofence.GeofenceTransitionDwell:
            	            gTransition = GeofenceTransition.Stayed;
            	            break;
            	        default:
            	            string message = string.Format("{0} - {1}", CrossGeofence.Id, "Invalid transition type");
            	            System.Diagnostics.Debug.WriteLine(message);
            	            gTransition = GeofenceTransition.Unknown;
            	            break;
            	    }
            	    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Transition: {1}", CrossGeofence.Id, gTransition));
            	    if (CrossGeofence.Current.GeofenceResults[geofence.RequestId].Transition != gTransition )
            	    {
            	   
            	        CrossGeofence.Current.GeofenceResults[geofence.RequestId].Transition = gTransition;
            	        
            	        CrossGeofence.GeofenceListener.OnRegionStateChanged(CrossGeofence.Current.GeofenceResults[geofence.RequestId]);
            	
            	        if (CrossGeofence.Current.Regions.ContainsKey(geofence.RequestId) && CrossGeofence.Current.Regions[geofence.RequestId].ShowNotification)
            	        {
            	
            	            string message = CrossGeofence.Current.GeofenceResults[geofence.RequestId].ToString();
            	           
            	            if(CrossGeofence.Current.Regions.ContainsKey(geofence.RequestId))
            	           {
            	             var region = CrossGeofence.Current.Regions[geofence.RequestId];
            	             switch(gTransition)
            	             {
            	               case GeofenceTransition.Entered:
            	                   if (!region.ShowEntryNotification)
            	                     return;
            	                   message=string.IsNullOrEmpty(region.NotificationEntryMessage)?message:region.NotificationEntryMessage;
            	                   break;
            	               case GeofenceTransition.Exited:
            	                   if (!region.ShowExitNotification)
            	                     return;
            	                   message=string.IsNullOrEmpty(region.NotificationExitMessage)?message:region.NotificationExitMessage;
            	                   break;
            	               case GeofenceTransition.Stayed:
            	                   if (!region.ShowStayNotification)
            	                     return;
            	                   message=string.IsNullOrEmpty(region.NotificationStayMessage)?message:region.NotificationStayMessage;
            	                   break;
            	
            	             }
            	           }
            	         
            	
            	           CreateNotification(context.ApplicationInfo.LoadLabel(context.PackageManager), message);
            	        }
            	
            	
            	        //Check if device has stayed in region asynchronosly
            	        //Commented because is already handled using DWELL on Android
            	        //CheckIfStayed(geofence.RequestId);
            	       
            	       
            	    }
            	}
            }
			catch (Java.Lang.Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, ex.ToString()));
			}
			catch (System.Exception ex) 
			{
				System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, ex.ToString()));
            }           
        }

        /*public async Task CheckIfStayed(string regionId)
        {
            Context context = Android.App.Application.Context;
            if (CrossGeofence.Current.GeofenceResults.ContainsKey(regionId) && CrossGeofence.Current.Regions.ContainsKey(regionId) && CrossGeofence.Current.Regions[regionId].NotifyOnStay && CrossGeofence.Current.GeofenceResults[regionId].Transition == GeofenceTransition.Entered && CrossGeofence.Current.Regions[regionId].StayedInThresholdDuration.TotalMilliseconds != 0)
            {
                await Task.Delay((int)CrossGeofence.Current.Regions[regionId].StayedInThresholdDuration.TotalMilliseconds);

                if (CrossGeofence.Current.GeofenceResults[regionId].LastExitTime == null && CrossGeofence.Current.GeofenceResults[regionId].Transition != GeofenceTransition.Stayed)
                {
                    CrossGeofence.Current.GeofenceResults[regionId].Transition = GeofenceTransition.Stayed;

                    CrossGeofence.GeofenceListener.OnRegionStateChanged(CrossGeofence.Current.GeofenceResults[regionId]);

                    if (CrossGeofence.Current.Regions[regionId].ShowNotification)
                    {
                        CreateNotification(context.ApplicationInfo.LoadLabel(context.PackageManager), string.IsNullOrEmpty(CrossGeofence.Current.Regions[regionId].NotificationStayMessage) ? CrossGeofence.Current.GeofenceResults[regionId].ToString() : CrossGeofence.Current.Regions[regionId].NotificationStayMessage);
                    }

                }
            }
        }*/
       
        /// <summary>
        /// Create local notification
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public void CreateNotification(string title, string message)
        {
           
            try
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

                // Set the icon resource if we have one
                if(CrossGeofence.LargeIconResource != null)
                    builder.SetLargeIcon(CrossGeofence.LargeIconResource);

                // Set the color if we have one
                if(CrossGeofence.Color != 0)
                    builder.SetColor(CrossGeofence.Color);


                NotificationManager notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);

                if (NotificationId >= NotificationMaxId)
                {
                    NotificationId = 0;
                }

                notificationManager.Notify(NotificationId++, builder.Build());

            }
            catch (Java.Lang.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, ex.ToString()));
            }
            catch (System.Exception ex1)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Id, ex1.ToString()));
            }
           
        }
    }
}

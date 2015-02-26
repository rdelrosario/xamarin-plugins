using Android.App;
using Android.Content;
using PushNotification.Plugin.Abstractions;
using System;
using System.Threading.Tasks;
using Android.Gms.Common;
using Java.Util.Logging;
using Android.Gms.Gcm;
using Android.Util;
using Java.Lang;
using Android.Content.PM;
using Android.OS;
using System.Collections.Generic;
using Android.Preferences;
using Android.Support.V4.App;
using Android.Media;
using Android;


namespace PushNotification.Plugin
{
	  /// <summary>
	  /// Implementation for Feature
	  /// </summary>
        [Service]
        public class PushNotificationImplementation : IntentService, IPushNotification
        {
            private const string GcmPreferencesKey = "GCMPreferences";

           

            public IPushNotificationListener Listener { get; set; }

            public string Token { get { return GetRegistrationId(); } }

            public void Register()
            {
                if (!CrossPushNotification.IsInitialized)
                {
              
                   throw NewPushNotificationNotInitializedException();
                }
             
                if (string.IsNullOrEmpty(CrossPushNotification.SenderId))
                {


                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Register - SenderId is missing.", PushNotificationKey.DomainName));
                 
                    if (CrossPushNotification.IsInitialized)
                    {
                        CrossPushNotification.PushNotificationListener.OnError(string.Format("{0} - Register - Sender Id is missing.", PushNotificationKey.DomainName), DeviceType.Android);
                    }
                    else
                    {
                        throw NewPushNotificationNotInitializedException();
                    }
                }
                else if (string.IsNullOrEmpty(Token))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Registering for Push Notifications", PushNotificationKey.DomainName));

                    InternalRegister();

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Already Registered for Push Notifications", PushNotificationKey.DomainName));

                }

            }

            public void Unregister()
            {
                if (!CrossPushNotification.IsInitialized)
                {

                    throw NewPushNotificationNotInitializedException();
                }
                InternalUnRegister();
            }

            protected override void OnHandleIntent(Intent intent)
            {

                Bundle extras = intent.Extras;
                

                if (extras != null && !extras.IsEmpty)
                {
                    System.Diagnostics.Debug.WriteLine(intent.Action);

                    if (intent.Action.Equals(PushNotificationKey.IntentFromGcmMessage))
                    {
                    /*    StoreRegistrationId(Android.App.Application.Context, extras.GetString("registration_id"));
                    }
                    else
                    {*/
                        System.Diagnostics.Debug.WriteLine(string.Format("{0} - Push Received", PushNotificationKey.DomainName));

                        System.Diagnostics.Debug.WriteLine(intent.Action);

                        var parameters = new Dictionary<string, object>();

                        foreach (var key in intent.Extras.KeySet())
                        {

                            parameters.Add(key, intent.Extras.Get(key));

                        }

                        Context context = Android.App.Application.Context;

                        if (CrossPushNotification.IsInitialized)
                        {
                            CrossPushNotification.PushNotificationListener.OnMessage(parameters, DeviceType.Android);
                        }
                        else
                        {
                            throw NewPushNotificationNotInitializedException();
                        }

                        try
                        {
                            string title = context.ApplicationInfo.LoadLabel(context.PackageManager);
                            string message = "";

                            if (!string.IsNullOrEmpty(CrossPushNotification.NotificationContentTextKey) && parameters.ContainsKey(CrossPushNotification.NotificationContentTextKey))
                            {
                                message = parameters[CrossPushNotification.NotificationContentTextKey].ToString();
                            }
                            else if (parameters.ContainsKey(PushNotificationKey.Message))
                            {
                                message = parameters[PushNotificationKey.Message].ToString();
                            }
                            else if (parameters.ContainsKey(PushNotificationKey.Subtitle))
                            {
                                message = parameters[PushNotificationKey.Subtitle].ToString();
                            }
                            else if (parameters.ContainsKey(PushNotificationKey.Text))
                            {
                                message = parameters[PushNotificationKey.Text].ToString();
                            }

                            if (!string.IsNullOrEmpty(CrossPushNotification.NotificationContentTitleKey) && parameters.ContainsKey(CrossPushNotification.NotificationContentTitleKey))
                            {
                                title = parameters[CrossPushNotification.NotificationContentTitleKey].ToString();

                            }
                            else if (parameters.ContainsKey(PushNotificationKey.Title))
                            {

                                if (!string.IsNullOrEmpty(message))
                                {
                                    title = parameters[PushNotificationKey.Title].ToString();
                                }
                                else
                                {
                                    message = parameters[PushNotificationKey.Title].ToString();
                                }
                            }

                            if (!parameters.ContainsKey(PushNotificationKey.Silent) || !System.Boolean.Parse(parameters[PushNotificationKey.Silent].ToString()))
                            {

                                  CreateNotification(title, message);

                            }

                        }
                        catch (Java.Lang.Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.ToString());
                        }
                        catch (System.Exception ex1)
                        {
                            System.Diagnostics.Debug.WriteLine(ex1.ToString());
                        }
                    }


                }
                // Release the wake lock provided by the WakefulBroadcastReceiver.
                PushNotificationsReceiver.CompleteWakefulIntent(intent);
            }
           
            private async Task InternalUnRegister()
            {

                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Unregistering push notifications", PushNotificationKey.DomainName));
                GoogleCloudMessaging gcm = GoogleCloudMessaging.GetInstance(Android.App.Application.Context);


                await Task.Run(() =>
                {
                    gcm.Unregister();
                });


               

                if (CrossPushNotification.IsInitialized)
                {
                    CrossPushNotification.PushNotificationListener.OnUnregistered(DeviceType.Android);
                }
                else
                {
                    throw NewPushNotificationNotInitializedException();
                }
                StoreRegistrationId(Android.App.Application.Context, string.Empty);

            }

            private async Task InternalRegister()
            {
                Context context = Android.App.Application.Context;

                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Registering push notifications", PushNotificationKey.DomainName));

                if (CrossPushNotification.SenderId == null)
                    throw new ArgumentException("No Sender Id Specified");

                try
                {
                    GoogleCloudMessaging gcm = GoogleCloudMessaging.GetInstance(context);

                    string regId = await Task.Run(() =>
                        {
                            return gcm.Register(CrossPushNotification.SenderId);
                        });

                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Device registered, registration ID=" + regId, PushNotificationKey.DomainName));


                
                    if (CrossPushNotification.IsInitialized)
                    {
                        CrossPushNotification.PushNotificationListener.OnRegistered(regId,DeviceType.Android);
                    }
                    else
                    {
                        throw NewPushNotificationNotInitializedException();
                    }
                    // Persist the regID - no need to register again.
                    StoreRegistrationId(context, regId);



                }
                catch (System.Exception ex)
                {

                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Error :" + ex.Message, PushNotificationKey.DomainName));
        
                    if (CrossPushNotification.IsInitialized)
                    {
                        CrossPushNotification.PushNotificationListener.OnError(string.Format("{0} - Register - " + ex.ToString(), PushNotificationKey.DomainName), DeviceType.Android);
                    }
                    else
                    {
                        throw NewPushNotificationNotInitializedException();
                    }
                }


            }
            PushNotificationNotInitializedException NewPushNotificationNotInitializedException()
            {
                string description = "CrossPushNotification Plugin is not initialized. Should initialize before use with CrossPushNotification Initialize method. Example:  CrossPushNotification.Initialize<CrossPushNotificationListener>()";

                return new PushNotificationNotInitializedException(description); 
            }


            private string GetRegistrationId()
            {
                string retVal = "";

                Context context = Android.App.Application.Context;

                ISharedPreferences prefs = GetGCMPreferences(context);

                string registrationId = prefs.GetString(PushNotificationKey.Token, "");

                if (string.IsNullOrEmpty(registrationId))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - - Registration not found.", PushNotificationKey.DomainName));

                    return retVal;
                }
                // Check if app was updated; if so, it must clear the registration ID
                // since the existing registration ID is not guaranteed to work with
                // the new app version.
                int registeredVersion = prefs.GetInt(PushNotificationKey.AppVersion, Integer.MinValue);
                int currentVersion = GetAppVersion(context);
                if (registeredVersion != currentVersion)
                {

                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - App version changed.", PushNotificationKey.DomainName));

                    return retVal;
                }

                retVal = registrationId;

                return retVal;
            }
            private ISharedPreferences GetGCMPreferences(Context context)
            {
                // This sample app persists the registration ID in shared preferences, but
                // how you store the registration ID in your app is up to you.

                return context.GetSharedPreferences(GcmPreferencesKey, FileCreationMode.Private);
            }

            private static int GetAppVersion(Context context)
            {
                try
                {
                    PackageInfo packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, 0);
                    return packageInfo.VersionCode;
                }
                catch (Android.Content.PM.PackageManager.NameNotFoundException e)
                {
                    // should never happen
                    throw new RuntimeException("Could not get package name: " + e);
                }

            }

            private void StoreRegistrationId(Context context, string regId)
            {
                ISharedPreferences prefs = GetGCMPreferences(context);
                int appVersion = GetAppVersion(context);

                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Saving regId on app version " + appVersion, PushNotificationKey.DomainName));

                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutString(PushNotificationKey.Token, regId);
                editor.PutInt(PushNotificationKey.AppVersion, appVersion);
                editor.Commit();
            }
        public static void CreateNotification(string title, string message)
        {
           
             NotificationCompat.Builder builder = null;
             Context context = Android.App.Application.Context;

             if(CrossPushNotification.SoundUri==null)
             {
                 CrossPushNotification.SoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
             }
             try
             {
                
                 if (CrossPushNotification.IconResource == null || CrossPushNotification.IconResource == 0 )
                 {
                     CrossPushNotification.IconResource = context.ApplicationInfo.Icon;
                 }
                 else
                 {
                     string name = context.Resources.GetResourceName(CrossPushNotification.IconResource);

                     if (name == null)
                     {
                         CrossPushNotification.IconResource = context.ApplicationInfo.Icon;

                     }
                 }
                 
             }
             catch (Android.Content.Res.Resources.NotFoundException ex)
             {
                 CrossPushNotification.IconResource = context.ApplicationInfo.Icon;
                 System.Diagnostics.Debug.WriteLine(ex.ToString());
             }


            Intent resultIntent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);

            //Intent resultIntent = new Intent(context, typeof(T));
           

             
            // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
            const int pendingIntentId = 0;
            PendingIntent resultPendingIntent = PendingIntent.GetActivity(context, pendingIntentId, resultIntent, PendingIntentFlags.OneShot);
          
            // Build the notification
            builder = new NotificationCompat.Builder(context)
                      .SetAutoCancel(true) // dismiss the notification from the notification area when the user clicks on it
                      .SetContentIntent(resultPendingIntent) // start up this activity when the user clicks the intent.
                      .SetContentTitle(title) // Set the title
                      .SetSound(CrossPushNotification.SoundUri)                           
                      .SetSmallIcon(CrossPushNotification.IconResource) // This is the icon to display
                      .SetContentText(message); // the message to display.

            NotificationManager notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            notificationManager.Notify(0, builder.Build());
       }


    }
}
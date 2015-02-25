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

            public event PushNotificationMessageEventHandler OnMessage = delegate { };

            public event PushNotificationRegistrationEventHandler OnRegistered = delegate { };

            public event PushNotificationUnregistrationEventHandler OnUnregistered = delegate { };

            public event PushNotificationErrorEventHandler OnError = delegate { };

            public string SenderId { get; set; }

            public string Token { get { return GetRegistrationId(); } }

            public void Register(string id = null)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    SenderId = id;
                }

                if (string.IsNullOrEmpty(SenderId))
                {


                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Register - SenderId is missing.", PushNotificationKey.DomainName));
                    OnError(this, new PushNotificationErrorEventArgs() { Message = string.Format("{0} - Register - Sender Id is missing.", PushNotificationKey.DomainName), DeviceType = DeviceType.Android });

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
                     
                        if (CrossPushNotification.Current is PushNotificationImplementation)
                        {
                            ((PushNotificationImplementation)CrossPushNotification.Current).OnMessageReceived(parameters);

                        }
                    }


                }
                // Release the wake lock provided by the WakefulBroadcastReceiver.
                PushNotificationsReceiver.CompleteWakefulIntent(intent);
            }
            public void OnMessageReceived(Dictionary<string, object> parameters)
            {
                Context context = Android.App.Application.Context;
                OnMessage(this, new PushNotificationMessageEventArgs() { Parameters = parameters, DeviceType = DeviceType.iOS });
                try
                {
                    string title = context.ApplicationInfo.LoadLabel(context.PackageManager);
                    string message = "";

                    if (!string.IsNullOrEmpty(PushNotificationHelper.NotificationContentTextKey)&&parameters.ContainsKey(PushNotificationHelper.NotificationContentTextKey))
                    {
                        message = parameters[PushNotificationHelper.NotificationContentTextKey].ToString();
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

                    if (!string.IsNullOrEmpty(PushNotificationHelper.NotificationContentTitleKey) && parameters.ContainsKey(PushNotificationHelper.NotificationContentTitleKey))
                    {
                        title = parameters[PushNotificationHelper.NotificationContentTitleKey].ToString();
                    
                    }else if (parameters.ContainsKey(PushNotificationKey.Title))
                    {
                       
                        if(!string.IsNullOrEmpty(message))
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
                       
                        PushNotificationHelper.CreateNotification(title, message);

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
            private async Task InternalUnRegister()
            {

                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Unregistering push notifications", PushNotificationKey.DomainName));
                GoogleCloudMessaging gcm = GoogleCloudMessaging.GetInstance(Android.App.Application.Context);


                await Task.Run(() =>
                {
                    gcm.Unregister();
                });


                OnUnregistered(this, new PushNotificationBaseEventArgs() { DeviceType = DeviceType.Android });

                StoreRegistrationId(Android.App.Application.Context, string.Empty);

            }

            private async Task InternalRegister()
            {
                Context context = Android.App.Application.Context;

                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Registering push notifications", PushNotificationKey.DomainName));

                if (SenderId == null)
                    throw new ArgumentException("No Sender Id Specified");

                try
                {
                    GoogleCloudMessaging gcm = GoogleCloudMessaging.GetInstance(context);

                    string regId = await Task.Run(() =>
                        {
                            return gcm.Register(SenderId);
                        });

                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Device registered, registration ID=" + regId, PushNotificationKey.DomainName));


                    OnRegistered(this, new PushNotificationRegistrationEventArgs() { DeviceType = DeviceType.Android, Token = regId });
                    // Persist the regID - no need to register again.
                    StoreRegistrationId(context, regId);

                }
                catch (System.Exception ex)
                {

                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Error :" + ex.Message, PushNotificationKey.DomainName));
                    OnError(this, new PushNotificationErrorEventArgs() { Message = string.Format("{0} - Register - " + ex.ToString(), PushNotificationKey.DomainName), DeviceType = DeviceType.Android });

                }


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

        }
	}
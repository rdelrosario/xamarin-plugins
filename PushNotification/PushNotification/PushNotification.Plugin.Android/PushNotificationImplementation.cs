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
using Android.Gms.Gcm.Iid;
using System.Threading;
using Java.IO;


namespace PushNotification.Plugin
{
	  /// <summary>
	  /// Implementation for Feature
	  /// </summary>
        public class PushNotificationImplementation : IPushNotification
        {
            private const string GcmPreferencesKey = "GCMPreferences";
            private int DefaultBackOffMilliseconds = 3000;
            const string Tag = "PushNotification";
           
            /// <summary>
            /// Push Notification Listener
            /// </summary>
            internal static IPushNotificationListener Listener { get; set; }

           /// <summary>
           /// GCM Token
           /// </summary>
            public string Token { get { return GetRegistrationId(); } }

            /// <summary>
            /// Register for Push Notifications
            /// </summary>
            public void Register()
            {

                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Register -  Registering push notifications", PushNotificationKey.DomainName));

                if (string.IsNullOrEmpty(CrossPushNotification.SenderId))
                {


                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Register - SenderId is missing.", PushNotificationKey.DomainName));
                 
                    CrossPushNotification.PushNotificationListener.OnError(string.Format("{0} - Register - Sender Id is missing.", PushNotificationKey.DomainName), DeviceType.Android);
             
                }
                else //if (string.IsNullOrEmpty(Token))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Register -  Registering for Push Notifications", PushNotificationKey.DomainName));
                    //ResetBackoff();
                    

                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            try
                            {
                               Intent intent = new Intent(Android.App.Application.Context, typeof(PushNotificationRegistrationIntentService));
                               Android.App.Application.Context.StartService(intent);
                            }
                            catch (System.Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Error :" + ex.Message, Tag));

                                CrossPushNotification.PushNotificationListener.OnError(string.Format("{0} - Register - " + ex.ToString(), Tag), DeviceType.Android);

                            }
                     
                        });

                    
                   

                }
                //else
                //{
                //    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Register -  Already Registered for Push Notifications", PushNotificationKey.DomainName));

                //}

            }

            /// <summary>
            /// Unregister push notifications
            /// </summary>
            public void Unregister()
            {


                ThreadPool.QueueUserWorkItem(state =>
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Unregister -  Unregistering push notifications", PushNotificationKey.DomainName));
                    try
                    {
                        InstanceID instanceID = InstanceID.GetInstance(Android.App.Application.Context);
                        instanceID.DeleteToken(CrossPushNotification.SenderId, GoogleCloudMessaging.InstanceIdScope);

                        CrossPushNotification.PushNotificationListener.OnUnregistered(DeviceType.Android);
                        PushNotificationImplementation.StoreRegistrationId(Android.App.Application.Context, string.Empty);

                    }catch(IOException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("{0} - Error :" + ex.Message, Tag));

                        CrossPushNotification.PushNotificationListener.OnError(string.Format("{0} - Unregister - " + ex.ToString(), Tag), DeviceType.Android);
              

                    }
                });
                 
                
               

            }



            private string GetRegistrationId()
            {
                string retVal = "";

                Context context = Android.App.Application.Context;

                ISharedPreferences prefs = GetGCMPreferences(context);

                string registrationId = prefs.GetString(PushNotificationKey.Token, string.Empty);

                if (string.IsNullOrEmpty(registrationId))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Registration not found.", PushNotificationKey.DomainName));

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
            internal static ISharedPreferences GetGCMPreferences(Context context)
            {
                // This sample app persists the registration ID in shared preferences, but
                // how you store the registration ID in your app is up to you.

                return context.GetSharedPreferences(GcmPreferencesKey, FileCreationMode.Private);
            }

            internal static int GetAppVersion(Context context)
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

            internal static void StoreRegistrationId(Context context, string regId)
            {
                ISharedPreferences prefs = GetGCMPreferences(context);
                int appVersion = GetAppVersion(context);

                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Saving token on app version " + appVersion, PushNotificationKey.DomainName));

                ISharedPreferencesEditor editor = prefs.Edit();
                editor.PutString(PushNotificationKey.Token, regId);
                editor.PutInt(PushNotificationKey.AppVersion, appVersion);
                editor.Commit();
            }


           /* internal void ResetBackoff()
            {
               // Logger.Debug("resetting backoff for " + context.PackageName);
                Context context = Android.App.Application.Context;
                SetBackoff(DefaultBackOffMilliseconds);
            }

            internal int GetBackoff()
            {
                Context context = Android.App.Application.Context;
                var prefs = GetGCMPreferences(context);
                return prefs.GetInt(PushNotificationKey.BackOffMilliseconds, DefaultBackOffMilliseconds);
            }

            internal void SetBackoff(int backoff)
            {
                Context context = Android.App.Application.Context;
                var prefs = GetGCMPreferences(context);
                var editor = prefs.Edit();
                editor.PutInt(PushNotificationKey.BackOffMilliseconds, backoff);
                editor.Commit();
            }*/

      
      

    }
}
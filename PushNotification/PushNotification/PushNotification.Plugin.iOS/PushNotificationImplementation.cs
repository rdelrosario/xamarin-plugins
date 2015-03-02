#if __UNIFIED__
using Foundation;
using UIKit;
#else
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif
using PushNotification.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace PushNotification.Plugin
{
  /// <summary>
  /// Implementation for PushNotification
  /// </summary>
	public class PushNotificationImplementation : IPushNotification,  IPushNotificationHandler
  {

		public string Token {
			get {
				return NSUserDefaults.StandardUserDefaults.StringForKey (PushNotificationKey.Token);
			}

		}

		public void Register()
		{
            if (!CrossPushNotification.IsInitialized)
            {

                throw NewPushNotificationNotInitializedException();
            }

			if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
			{
				UIUserNotificationType userNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
				UIUserNotificationSettings settings = UIUserNotificationSettings.GetSettingsForTypes(userNotificationTypes, null);
				UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
			}
			else
			{
				UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
				UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
			} 
		}

		public void Unregister()
		{
            if (!CrossPushNotification.IsInitialized)
            {

                throw NewPushNotificationNotInitializedException();
            }
			UIApplication.SharedApplication.UnregisterForRemoteNotifications();

            

		}

		#region IPushNotificationListener implementation

		public void OnMessageReceived (NSDictionary userInfo)
		{
			var parameters = new Dictionary<string, object>();

			foreach (NSString key in userInfo.Keys)
			{
				parameters.Add(key, userInfo.ValueForKey(key));
			}
          
            if (CrossPushNotification.IsInitialized)
            {
                CrossPushNotification.PushNotificationListener.OnMessage(parameters, DeviceType.iOS);
            }else
            {
                throw NewPushNotificationNotInitializedException();
            }
		}

		public void OnErrorReceived (NSError error)
		{
			Debug.WriteLine("{0} - Registration Failed.", PushNotificationKey.DomainName);
		
            if (CrossPushNotification.IsInitialized)
            {
                CrossPushNotification.PushNotificationListener.OnError(error.LocalizedDescription, DeviceType.iOS);
            }
            else
            {
                throw NewPushNotificationNotInitializedException();
            }
		}

		public void OnRegisteredSuccess (NSData token)
		{
			Debug.WriteLine("{0} - Succesfully Registered.", PushNotificationKey.DomainName);


			string trimmedDeviceToken =token.Description;
			if (!string.IsNullOrWhiteSpace(trimmedDeviceToken))
			{
				trimmedDeviceToken = trimmedDeviceToken.Trim('<');
				trimmedDeviceToken = trimmedDeviceToken.Trim('>');
                trimmedDeviceToken = trimmedDeviceToken.Trim();
			}
			Console.WriteLine("{0} - Token: {1}", PushNotificationKey.DomainName, trimmedDeviceToken);


            if (CrossPushNotification.IsInitialized)
            {
                CrossPushNotification.PushNotificationListener.OnRegistered(trimmedDeviceToken, DeviceType.iOS);
            }
            else
            {
                throw NewPushNotificationNotInitializedException();
            }


            
            NSUserDefaults.StandardUserDefaults.SetString (PushNotificationKey.Token,trimmedDeviceToken);
			NSUserDefaults.StandardUserDefaults.Synchronize ();
		}

		public void OnUnregisteredSuccess ()
		{
            NSUserDefaults.StandardUserDefaults.SetString(PushNotificationKey.Token, "");
            NSUserDefaults.StandardUserDefaults.Synchronize();

            if (CrossPushNotification.IsInitialized)
            {
                CrossPushNotification.PushNotificationListener.OnUnregistered(DeviceType.iOS);
            }
            else
            {
                throw NewPushNotificationNotInitializedException();
            }

        }

        PushNotificationNotInitializedException NewPushNotificationNotInitializedException()
        {
            string description = "CrossPushNotification Plugin is not initialized. Should initialize before use on FinishedLaunching method of AppDelegate class. Example:  CrossPushNotification.Initialize<CrossPushNotificationListener>()";

            return new PushNotificationNotInitializedException(description);
        }

		#endregion
  }
}
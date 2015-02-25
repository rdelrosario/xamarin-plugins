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
	public class PushNotificationImplementation : IPushNotification,  IPushNotificationListener
  {
		public event PushNotificationMessageEventHandler OnMessage = delegate { };

		public event PushNotificationRegistrationEventHandler OnRegistered = delegate { };

		public event PushNotificationUnregistrationEventHandler OnUnregistered = delegate { };

		public event PushNotificationErrorEventHandler OnError = delegate { };


		public string Token {
			get {
				return NSUserDefaults.StandardUserDefaults.StringForKey (PushNotificationKey.Token);
			}

		}


		public void Register(string id = null)
		{
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

			UIApplication.SharedApplication.UnregisterForRemoteNotifications();
			OnUnregistered(this, new PushNotificationBaseEventArgs() { DeviceType = DeviceType.iOS });

		}

		#region IPushNotificationListener implementation

		public void OnMessageReceived (NSDictionary userInfo)
		{
			var parameters = new Dictionary<string, object>();

			foreach (NSString key in userInfo.Keys)
			{
				parameters.Add(key, userInfo.ValueForKey(key));
			}
            OnMessage(this, new PushNotificationMessageEventArgs() { Parameters = parameters, DeviceType = DeviceType.iOS });

		}

		public void OnErrorReceived (NSError error)
		{
			Debug.WriteLine("{0} - Registration Failed.", PushNotificationKey.DomainName);
		

            OnError(this, new PushNotificationErrorEventArgs() { Message = error.LocalizedDescription, DeviceType = DeviceType.iOS });


		}

		public void OnRegisteredSuccess (NSData token)
		{
			Debug.WriteLine("{0} - Succesfully Registered.", PushNotificationKey.DomainName);


			string trimmedDeviceToken =token.Description;
			if (!string.IsNullOrWhiteSpace(trimmedDeviceToken))
			{
				trimmedDeviceToken = trimmedDeviceToken.Trim('<');
				trimmedDeviceToken = trimmedDeviceToken.Trim('>');
			}
			Console.WriteLine("{0} - Token: {1}", PushNotificationKey.DomainName, trimmedDeviceToken);
			OnRegistered(this, new PushNotificationRegistrationEventArgs() { Token = trimmedDeviceToken, DeviceType = DeviceType.iOS });
			NSUserDefaults.StandardUserDefaults.SetString (PushNotificationKey.Token,trimmedDeviceToken);
			NSUserDefaults.StandardUserDefaults.Synchronize ();
		}

		public void OnUnregisteredSuccess ()
		{
			OnUnregistered(this,new PushNotificationBaseEventArgs() { DeviceType = DeviceType.iOS });
		}

		#endregion
  }
}
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
using Newtonsoft.Json.Linq;


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


		}

      private static string DictionaryToJson(NSDictionary dictionary)
      {
          NSError error;
          var json = NSJsonSerialization.Serialize(dictionary, NSJsonWritingOptions.PrettyPrinted, out error);

          return json.ToString(NSStringEncoding.UTF8);
      }

		#region IPushNotificationListener implementation

		public void OnMessageReceived (NSDictionary userInfo)
		{
			var parameters = new Dictionary<string, object>();
		    var json = DictionaryToJson(userInfo);
		    JObject values = JObject.Parse(json);

		    var keyAps = new NSString("aps");

            if (userInfo.ContainsKey(keyAps))
		    {
                NSDictionary aps = userInfo.ValueForKey(keyAps) as NSDictionary;

                if (aps != null)
                {
                    foreach (var apsKey in aps)
                    {
                        parameters.Add(apsKey.Key.ToString(), apsKey.Value);
                        JToken temp;
                        if (!values.TryGetValue(apsKey.Key.ToString(), out temp))
                            values.Add(apsKey.Key.ToString(), apsKey.Value.ToString());
                    }
                }
		    }

            CrossPushNotification.PushNotificationListener.OnMessage(values, DeviceType.iOS);

		}

		public void OnErrorReceived (NSError error)
		{
			Debug.WriteLine("{0} - Registration Failed.", PushNotificationKey.DomainName);
		
            CrossPushNotification.PushNotificationListener.OnError(error.LocalizedDescription, DeviceType.iOS);
         
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
                trimmedDeviceToken = trimmedDeviceToken.Replace(" ","");
			}
			Console.WriteLine("{0} - Token: {1}", PushNotificationKey.DomainName, trimmedDeviceToken);


            CrossPushNotification.PushNotificationListener.OnRegistered(trimmedDeviceToken, DeviceType.iOS);
            



            NSUserDefaults.StandardUserDefaults.SetString(trimmedDeviceToken, PushNotificationKey.Token);
			NSUserDefaults.StandardUserDefaults.Synchronize ();
		}

		public void OnUnregisteredSuccess ()
		{
            NSUserDefaults.StandardUserDefaults.SetString(string.Empty, PushNotificationKey.Token);
            NSUserDefaults.StandardUserDefaults.Synchronize();

            CrossPushNotification.PushNotificationListener.OnUnregistered(DeviceType.iOS);
           

        }

		#endregion
  }
}
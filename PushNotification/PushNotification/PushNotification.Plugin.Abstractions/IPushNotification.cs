using System;
using System.Collections.Generic;

namespace PushNotification.Plugin.Abstractions
{
  /// <summary>
  /// Interface for PushNotification
  /// </summary>
  public interface IPushNotification
  {
      
      event PushNotificationMessageEventHandler OnMessage;
	  event PushNotificationRegistrationEventHandler OnRegistered;
	  event PushNotificationUnregistrationEventHandler OnUnregistered;
      event PushNotificationErrorEventHandler OnError;

	  string Token { get; }
     
      void Register(string id = null);
      void Unregister();

 
  }

  public class PushNotificationMessageEventArgs : PushNotificationBaseEventArgs
      {

          public IDictionary<string, object> Parameters { get; set; }

      }

	  public class PushNotificationBaseEventArgs : EventArgs
	  {


		public DeviceType DeviceType { get; set; }

	  }


	  
	  public class PushNotificationRegistrationEventArgs : PushNotificationBaseEventArgs 
      {

          public string Token { get; set; }
          

      }

      public class PushNotificationErrorEventArgs : PushNotificationBaseEventArgs
      {

          public string Message { get; set; }


      }

         /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public delegate void PushNotificationMessageEventHandler(object sender, PushNotificationMessageEventArgs e);
      public delegate void PushNotificationRegistrationEventHandler(object sender, PushNotificationRegistrationEventArgs e);
	  public delegate void PushNotificationUnregistrationEventHandler(object sender, PushNotificationBaseEventArgs  e);
      public delegate void PushNotificationErrorEventHandler(object sender, PushNotificationErrorEventArgs e);
  
}

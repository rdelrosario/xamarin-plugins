using PushNotification.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace $rootnamespace$.Helpers
{
    //Class to handle push notifications listens to events such as registration, unregistration, message arrival and errors.
    public class  CrossPushNotificationListener : IPushNotificationListener
    {

        void IPushNotificationListener.OnMessage(IDictionary<string, object> Parameters, DeviceType deviceType)
        {
            Debug.WriteLine("Message Arrived");
        }

        void IPushNotificationListener.OnRegistered(string Token, DeviceType deviceType)
        {
            Debug.WriteLine(string.Format("Push Notification - Device Registered - Token : {0}", Token));
        }

        void IPushNotificationListener.OnUnregistered(DeviceType deviceType)
        {
            Debug.WriteLine("Push Notification - Device Unnregistered");
       
        }

        void IPushNotificationListener.OnError(string message, DeviceType deviceType)
        {
            Debug.WriteLine(string.Format("Push notification error - {0}",message));
        }
    }
}

using PushNotification.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
namespace PushNotification.Plugin
{
    /// <summary>
    /// Push Events Listener
    /// </summary>
    public interface IPushNotificationListener
    {
        /// <summary>
        /// On Message Received
        /// </summary>
        /// <param name="values"></param>
        /// <param name="deviceType"></param>
        void OnMessage(JObject values, DeviceType deviceType);
        /// <summary>
        /// On Registered
        /// </summary>
        /// <param name="token"></param>
        /// <param name="deviceType"></param>
        void OnRegistered(string token, DeviceType deviceType);
        /// <summary>
        /// On Unregistered
        /// </summary>
        /// <param name="deviceType"></param>
        void OnUnregistered(DeviceType deviceType);
        /// <summary>
        /// OnError
        /// </summary>
        void OnError(string message,DeviceType deviceType);
        /// <summary>
        /// Should Show Notification
        /// </summary>
        bool ShouldShowNotification();
    }

}

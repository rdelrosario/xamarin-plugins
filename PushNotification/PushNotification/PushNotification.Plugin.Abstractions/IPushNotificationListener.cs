using PushNotification.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="Parameters"></param>
        /// <param name="deviceType"></param>
        void OnMessage(IDictionary<string, object> Parameters,DeviceType deviceType);
        /// <summary>
        /// On Registered
        /// </summary>
        /// <param name="Token"></param>
        /// <param name="deviceType"></param>
        void OnRegistered(string Token, DeviceType deviceType);
        /// <summary>
        /// On Unregistered
        /// </summary>
        /// <param name="deviceType"></param>
        void OnUnregistered( DeviceType deviceType);
        /// <summary>
        /// OnError
        /// </summary>
        void OnError(string message,DeviceType deviceType);
        /// <summary>
        /// Should Show Notification
        /// </summary>
        bool ShouldShowNotification();
    }

    /// <summary>
    /// Push Events Listener
    /// </summary>
    public interface IPushNotificationListener<T> : IPushNotificationListener
    {
        /// <summary>
        /// On Message Received
        /// </summary>
        /// <param name="Parameters"></param>
        /// <param name="deviceType"></param>
        void OnMessage(T Parameters, DeviceType deviceType);
       
    }
}

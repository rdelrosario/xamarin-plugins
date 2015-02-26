using PushNotification.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushNotification.Plugin
{
    public interface IPushNotificationListener
    {
        void OnMessage(IDictionary<string, object> Parameters,DeviceType deviceType);
        void OnRegistered(string Token, DeviceType deviceType);
        void OnUnregistered( DeviceType deviceType);
        void OnError(string message,DeviceType deviceType);
    }
}

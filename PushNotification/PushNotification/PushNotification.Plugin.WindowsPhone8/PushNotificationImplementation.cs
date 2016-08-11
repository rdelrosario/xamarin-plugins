using PushNotification.Plugin.Abstractions;
using System;
using System.Collections.Generic;


namespace PushNotification.Plugin
{
    /// <summary>
    /// Implementation for PushNotification
    /// </summary>
    public class PushNotificationImplementation : IPushNotification
    {
        public string Token
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Register()
        {
            throw new NotImplementedException();
        }

        public void Unregister()
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushNotification.Plugin.Abstractions
{
    /// <summary>
    /// Push Notification not Initialized Exception class
    /// </summary>
    public class PushNotificationNotInitializedException : Exception
    {
          /// <summary>
          /// Default Contructor
          /// </summary>
          public PushNotificationNotInitializedException()
          {
          }
          /// <summary>
          /// Constructor with message
          /// </summary>
          public PushNotificationNotInitializedException(string message): base(message)
          {
          }
    }
}

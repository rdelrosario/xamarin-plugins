using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushNotification.Plugin.Abstractions
{
    public class PushNotificationNotInitializedException : Exception
    {
          public PushNotificationNotInitializedException()
          {
          }

          public PushNotificationNotInitializedException(string message): base(message)
          {
          }
    }
}

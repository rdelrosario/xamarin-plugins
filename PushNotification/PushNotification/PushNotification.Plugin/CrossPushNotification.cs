using PushNotification.Plugin.Abstractions;
using System;

namespace PushNotification.Plugin
{
  /// <summary>
  /// Cross platform PushNotification implemenations
  /// </summary>
  public class CrossPushNotification
  {
    static Lazy<IPushNotification> Implementation = new Lazy<IPushNotification>(() => CreatePushNotification(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// Current settings to use
    /// </summary>
    public static IPushNotification Current
    {
      get
      {
        var ret = Implementation.Value;
        if (ret == null)
        {
          throw NotImplementedInReferenceAssembly();
        }
        return ret;
      }
    }

    static IPushNotification CreatePushNotification()
    {
#if PORTABLE
        return null;
#else
        return new PushNotificationImplementation();
#endif
    }

    internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}

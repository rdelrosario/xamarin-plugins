using PushNotification.Plugin.Abstractions;
using System;
using System.Diagnostics;

namespace PushNotification.Plugin
{
  /// <summary>
  /// Cross platform PushNotification implemenations
  /// </summary>
  public class CrossPushNotification
  {

    static Lazy<IPushNotification> Implementation = new Lazy<IPushNotification>(() => CreatePushNotification(), System.Threading.LazyThreadSafetyMode.PublicationOnly);
    public static bool IsInitialized { get { return (PushNotificationListener != null);  } }
    public static IPushNotificationListener PushNotificationListener { get; private set; }
  
#if __ANDROID__
    public static string SenderId { get; set; }
    public static string NotificationContentTitleKey { get; set; }
    public static string NotificationContentTextKey { get; set; }
    public static int IconResource { get; set; }
    public static Android.Net.Uri SoundUri { get; set; }
    public static void Initialize<T>(string senderId)
           where T : IPushNotificationListener, new()
#endif
#if __IOS__
    public static void Initialize<T>()
            where T : IPushNotificationListener, new()
#endif



#if __MOBILE__
    {
       
#if __ANDROID__
        
        SenderId = senderId;

#endif

        if (PushNotificationListener == null)
        {
            PushNotificationListener = (IPushNotificationListener)Activator.CreateInstance(typeof(T));
            Debug.WriteLine("PushNotification plugin initialized.");
        }
        else
        {
            Debug.WriteLine("PushNotification plugin already initialized.");
        }
       
    }
#endif




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

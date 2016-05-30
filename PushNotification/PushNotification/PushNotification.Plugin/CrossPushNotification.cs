using PushNotification.Plugin.Abstractions;
using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    public static string NotificationContentDataKey { get; set; }
    public static int IconResource { get; set; }
    public static Android.Net.Uri SoundUri { get; set; }
#endif

    public static void Initialize<T>(
      T listener
#if __ANDROID__
      , string senderId
#endif
    )
      where T : IPushNotificationListener
    {
#if __ANDROID__
      SenderId = senderId;
#endif

      if (PushNotificationListener == null)
      {
         PushNotificationListener = listener;
         Debug.WriteLine("PushNotification plugin initialized.");
      }
      else
      {
         Debug.WriteLine("PushNotification plugin already initialized.");
      }
    }

    public static void Initialize<T>(
#if __ANDROID__
      string senderId
#endif
    )
      where T : IPushNotificationListener, new()
    {
      Initialize<T>(
        new T()
#if __ANDROID__
        , senderId    
#endif
      );
}

        /// <summary>
        /// Current settings to use
        /// </summary>
        public static IPushNotification Current
    {
      get
      {
          //Should always initialize plugin before use
        if (!CrossPushNotification.IsInitialized)
        {
              throw NewPushNotificationNotInitializedException();
        }
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

    internal static PushNotificationNotInitializedException NewPushNotificationNotInitializedException()
    {
        string description = "CrossPushNotification Plugin is not initialized. Should initialize before use with CrossPushNotification Initialize method. Example:  CrossPushNotification.Initialize<CrossPushNotificationListener>()";

        return new PushNotificationNotInitializedException(description);
    }

     internal static bool ValidateJSON(string s)
        {
            try
            {
                JObject.Parse(s);
                return true;
            }
            catch (JsonReaderException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }
        }

      /*internal static bool IsGenericTypeOf(Type genericType, Type someType)
        {
            if (someType.IsGenericType
                    && genericType == someType.GetGenericTypeDefinition()) return true;

            return someType.BaseType != null
                    && IsGenericTypeOf(genericType, someType.BaseType);
        }*/
    }
}

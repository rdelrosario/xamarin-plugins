using Geofence.Plugin.Abstractions;
using System;
using System.Diagnostics;

namespace Geofence.Plugin
{
  /// <summary>
  /// Cross platform Geofence implemenations
  /// </summary>
  public class CrossGeofence
  {
    static Lazy<IGeofence> Implementation = new Lazy<IGeofence>(() => CreateGeofence(), System.Threading.LazyThreadSafetyMode.PublicationOnly);
    public static bool IsInitialized { get { return (GeofenceListener != null); } }

    public const string Tag = "CrossGeofence";
    public static IGeofenceListener GeofenceListener { get; private set; }

    public static bool EnableLocalNotifications { get; set; }
    public static GeofencePriority GeofencePriority { get; set; }
    public static float SmallestDisplacement { get; set; }
    public static double RegionMinimumDuration { get; set; }

    #if __ANDROID__
      public static int IconResource { get; set; }
      public static Android.Net.Uri SoundUri { get; set; }
      public static bool EnableMonitoringRestore { get; set; }
      public static bool EnableLocationUpdates { get; set; }
      public static int LocationUpdatesInterval { get; set; }
      public static int FastestLocationUpdatesInterval { get; set; }
     
    
   #endif


    public static void Initialize<T>()
     where T : IGeofenceListener, new()
    {

        if (GeofenceListener == null)
        {
            GeofenceListener = (IGeofenceListener)Activator.CreateInstance(typeof(T));
            Debug.WriteLine("Geofence plugin initialized.");
        }
        else
        {
            Debug.WriteLine("Geofence plugin already initialized.");
        }
       
    }
    /// <summary>
    /// Current settings to use
    /// </summary>
    public static IGeofence Current
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

    static IGeofence CreateGeofence()
    {
#if PORTABLE
        return null;
#else
        return new GeofenceImplementation();
#endif
    }

    internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}

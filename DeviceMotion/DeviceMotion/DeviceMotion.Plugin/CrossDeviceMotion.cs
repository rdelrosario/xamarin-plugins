using DeviceMotion.Plugin.Abstractions;
using System;

namespace DeviceMotion.Plugin
{
  /// <summary>
  /// Cross platform DeviceMotion implemenations
  /// </summary>
  public class CrossDeviceMotion
  {
    static Lazy<IDeviceMotion> Implementation = new Lazy<IDeviceMotion>(() => CreateDeviceMotion(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// Current settings to use
    /// </summary>
    public static IDeviceMotion Current
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

    static IDeviceMotion CreateDeviceMotion()
    {
#if PORTABLE
        return null;
#else
        return new DeviceMotionImplementation();
#endif
    }

    internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}

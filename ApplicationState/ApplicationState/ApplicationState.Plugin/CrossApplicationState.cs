using ApplicationState.Plugin.Abstractions;
using System;

namespace ApplicationState.Plugin
{
  /// <summary>
  /// Cross platform ApplicationState implemenations
  /// </summary>
  public class CrossApplicationState
  {
    static Lazy<IApplicationState> Implementation = new Lazy<IApplicationState>(() => CreateApplicationState(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// Current settings to use
    /// </summary>
    public static IApplicationState Current
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

    static IApplicationState CreateApplicationState()
    {
#if PORTABLE
        return null;
#else
        return new ApplicationStateImplementation();
#endif
    }

    internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}

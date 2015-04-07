using ApplicationState.Plugin.Abstractions;
#if __UNIFIED__
using UIKit;
#else
using MonoTouch.UIKit;
#endif

using System;


namespace ApplicationState.Plugin
{
  /// <summary>
  /// Implementation for ApplicationState
  /// </summary>
  public class ApplicationStateImplementation : IApplicationState
  {
      public bool IsBackground { get { return !IsForeground; } }
      public bool IsForeground 
      { 
          get 
          {
              UIApplicationState state = UIApplication.SharedApplication.ApplicationState;
              return   (state == UIApplicationState.Active);
          } 
      }
  }
}
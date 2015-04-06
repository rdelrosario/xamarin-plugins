using ApplicationState.Plugin.Abstractions;
using System;
using Android.Content;
using Android.App;
using System.Collections.Generic;

namespace ApplicationState.Plugin
{
  /// <summary>
  /// Implementation for Feature
  /// </summary>
  public class ApplicationStateImplementation : IApplicationState
  {
      public bool IsBackground { get { return !IsForeground; } }
      public bool IsForeground { get { return IsApplicationRunningOnBackground(); } }
      bool IsApplicationRunningOnBackground()
      {
          bool retVal = false;
      
          // check with the first task(task in the foreground)
          // in the returned list of tasks

          Context context = Android.App.Application.Context;
          ActivityManager activityManager = (ActivityManager)context.GetSystemService(Context.ActivityService);
          IList<Android.App.ActivityManager.RunningTaskInfo> services = activityManager.GetRunningTasks(int.MaxValue);
          if (services.Count >0 &&services[0].TopActivity.PackageName.ToString().Equals(context.PackageName.ToString(),StringComparison.OrdinalIgnoreCase))
          {
              // your application is running in the background
              retVal= true;
          }
          return retVal;
      }
  }
}
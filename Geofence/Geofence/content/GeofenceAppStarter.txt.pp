using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Geofence.Plugin;

namespace $rootnamespace$
{
    //This is a starting point application class so that geofence events can be handle even when application is closed.

    [Application]
    public class GeofenceAppStarter : Application
    {
		public static Context AppContext;

		public GeofenceAppStarter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{

		}

		public override void OnCreate()
		{
			base.OnCreate();

			AppContext = this.ApplicationContext;
                        
            //TODO: Initialize CrossGeofence Plugin
            //TODO: Specify the listener class implementing IGeofenceListener interface in the Initialize generic
            //CrossGeofence.Initialize<CrossGeofenceListener>();
	        //CrossGeofence.GeofenceListener.OnAppStarted();
            //Start a sticky service to keep receiving geofence events when app is closed.
			StartService();
		}

		public static void StartService()
		{
			AppContext.StartService(new Intent(AppContext, typeof(GeofenceService)));

			if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
			{
		
				PendingIntent pintent = PendingIntent.GetService(AppContext, 0, new Intent(AppContext, typeof(GeofenceService)), 0);
				AlarmManager alarm = (AlarmManager)AppContext.GetSystemService(Context.AlarmService);
				alarm.Cancel(pintent);
			}
		}

		public static void StopService()
		{
			AppContext.StopService(new Intent(AppContext, typeof(GeofenceService)));
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
			{
			    PendingIntent pintent = PendingIntent.GetService(AppContext, 0, new Intent(AppContext, typeof(GeofenceService)), 0);
			    AlarmManager alarm = (AlarmManager)AppContext.GetSystemService(Context.AlarmService);
			    alarm.Cancel(pintent);
			}
		}

    }
}

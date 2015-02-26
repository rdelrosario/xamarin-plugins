using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using PushNotification.Plugin;

namespace $rootnamespace$
{
    //This is a starting point application class so that push notifications can be handle even when push is closed.

    [Application]
    public class PushNotificationAppStarter : Application
    {
		public static Context AppContext;

		public PushNotificationAppStarter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{

		}

		public override void OnCreate()
		{
			base.OnCreate();

			AppContext = this.ApplicationContext;
                        
             //TODO: Initialize CrossPushNotification Plugin
             //TODO: Replace string parameter with your Android SENDER ID
             //TODO: Specify the listener class implementing IPushNotificationListener interface in the Initialize generic
             //CrossPushNotification.Initialize<CrossPushNotificationListener>("<ANDROID SENDER ID>");

			StartPushService();
		}

		public static void StartPushService()
		{
			AppContext.StartService(new Intent(AppContext, typeof(PushNotificationService)));

			if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
			{
		
				PendingIntent pintent = PendingIntent.GetService(AppContext, 0, new Intent(AppContext, typeof(PushNotificationService)), 0);
				AlarmManager alarm = (AlarmManager)AppContext.GetSystemService(Context.AlarmService);
				alarm.Cancel(pintent);
			}
		}

		public static void StopPushService()
		{
			AppContext.StopService(new Intent(AppContext, typeof(PushNotificationService)));
                        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
			{
			    PendingIntent pintent = PendingIntent.GetService(AppContext, 0, new Intent(AppContext, typeof(PushNotificationService)), 0);
			    AlarmManager alarm = (AlarmManager)AppContext.GetSystemService(Context.AlarmService);
			    alarm.Cancel(pintent);
			}
		}

    }
}

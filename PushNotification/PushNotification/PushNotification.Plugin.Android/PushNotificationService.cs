using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PushNotification.Plugin
{
    [Service]
    public class PushNotificationService : Service
    {
        public override void OnCreate()
        {
            base.OnCreate();

            System.Diagnostics.Debug.WriteLine("Push Notification Service - Created");
        }

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            System.Diagnostics.Debug.WriteLine("Push Notification Service - Started");
            return StartCommandResult.Sticky;
        }

        public override Android.OS.IBinder OnBind(Android.Content.Intent intent)
        {
            System.Diagnostics.Debug.WriteLine("Push Notification Service - Binded");
            return null;
        }

        public override void OnDestroy()
        {
            System.Diagnostics.Debug.WriteLine("Push Notification Service - Destroyed");
            base.OnDestroy();
        }
    }
}
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
using Android.Support.V4.App;
using Android.Media;

namespace PushNotification.Plugin
{
    public static class PushNotificationHelper
    {
        public static string NotificationContentTitleKey { get; set; }
        public static string NotificationContentTextKey { get; set; }
        public static int IconResource { get; set; }
        public static Android.Net.Uri SoundUri { get; set; }

       
        public static void CreateNotification(string title, string message)
        {
           
             NotificationCompat.Builder builder = null;
             Context context = Android.App.Application.Context;

             if(SoundUri==null)
             {
                 SoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
             }
             try
             {
                
                 if (IconResource == null || IconResource == 0 )
                 {
                     IconResource = context.ApplicationInfo.Icon;
                 }
                 else
                 {
                     String name = context.Resources.GetResourceName(IconResource);

                     if (name == null)
                     {
                         IconResource = context.ApplicationInfo.Icon;

                     }
                 }
                 
             }
             catch (Android.Content.Res.Resources.NotFoundException ex)
             {
                 IconResource = context.ApplicationInfo.Icon;
                 System.Diagnostics.Debug.WriteLine(ex.ToString());
             }


            Intent resultIntent = context.PackageManager.GetLaunchIntentForPackage(context.PackageName);

            //Intent resultIntent = new Intent(context, typeof(T));
           

             
            // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
            const int pendingIntentId = 0;
            PendingIntent resultPendingIntent = PendingIntent.GetActivity(context, pendingIntentId, resultIntent, PendingIntentFlags.OneShot);
          
            // Build the notification
            builder = new NotificationCompat.Builder(context)
                      .SetAutoCancel(true) // dismiss the notification from the notification area when the user clicks on it
                      .SetContentIntent(resultPendingIntent) // start up this activity when the user clicks the intent.
                      .SetContentTitle(title) // Set the title
                      .SetSound(SoundUri)                           
                      .SetSmallIcon(IconResource) // This is the icon to display
                      .SetContentText(message); // the message to display.

            NotificationManager notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            notificationManager.Notify(0, builder.Build());
          }

	  }
    
}
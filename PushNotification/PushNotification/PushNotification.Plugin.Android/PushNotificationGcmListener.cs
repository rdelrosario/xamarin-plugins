using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PushNotification.Plugin.Abstractions;
using Android.Gms.Gcm;
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
    /// <summary>
    /// Push Notification Message Handler
    /// </summary>
    [Service(Exported=false)]
    [IntentFilter(new string[] { "com.google.android.c2dm.intent.RECEIVE" })]
    public class PushNotificationGcmListener : GcmListenerService
    {
        /// <summary>
        /// Called when message is received.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="extras"></param>
        public override void OnMessageReceived(string from,Bundle extras)
        {
            if (extras != null && !extras.IsEmpty)
            {
              
                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - GCM Listener - Push Received", PushNotificationKey.DomainName));

                   

                    var parameters = new Dictionary<string, object>();

                    foreach (var key in extras.KeySet())
                    {

                        parameters.Add(key, extras.Get(key));
                        
                        System.Diagnostics.Debug.WriteLine(string.Format("{0} - GCM Listener - Push Params {1} : {2}", PushNotificationKey.DomainName, key, extras.Get(key)));

                   
                    }

                    Context context = Android.App.Application.Context;

                    if (CrossPushNotification.IsInitialized)
                    {
                        CrossPushNotification.PushNotificationListener.OnMessage(parameters, DeviceType.Android);
                    }
                    else
                    {
                        throw CrossPushNotification.NewPushNotificationNotInitializedException();
                    }

                    try
                    {
                        int notifyId = 0;
                        string title = context.ApplicationInfo.LoadLabel(context.PackageManager);
                        string message = "";
                        string tag = "";

                        if (!string.IsNullOrEmpty(CrossPushNotification.NotificationContentTextKey) && parameters.ContainsKey(CrossPushNotification.NotificationContentTextKey))
                        {
                            message = parameters[CrossPushNotification.NotificationContentTextKey].ToString();
                        }
                        else if (parameters.ContainsKey(PushNotificationKey.Message))
                        {
                            message = parameters[PushNotificationKey.Message].ToString();
                        }
                        else if (parameters.ContainsKey(PushNotificationKey.Subtitle))
                        {
                            message = parameters[PushNotificationKey.Subtitle].ToString();
                        }
                        else if (parameters.ContainsKey(PushNotificationKey.Text))
                        {
                            message = parameters[PushNotificationKey.Text].ToString();
                        }

                        if (!string.IsNullOrEmpty(CrossPushNotification.NotificationContentTitleKey) && parameters.ContainsKey(CrossPushNotification.NotificationContentTitleKey))
                        {
                            title = parameters[CrossPushNotification.NotificationContentTitleKey].ToString();

                        }
                        else if (parameters.ContainsKey(PushNotificationKey.Title))
                        {

                            if (!string.IsNullOrEmpty(message))
                            {
                                title = parameters[PushNotificationKey.Title].ToString();
                            }
                            else
                            {
                                message = parameters[PushNotificationKey.Title].ToString();
                            }
                        }
                        if (parameters.ContainsKey(PushNotificationKey.Id))
                        {
                            var str = parameters[PushNotificationKey.Id].ToString();
                            try
                            {
                                notifyId = Convert.ToInt32(str);
                            }
                            catch (System.Exception)
                            {
                                // Keep the default value of zero for the notify_id, but log the conversion problem.
                                System.Diagnostics.Debug.WriteLine("Failed to convert {0} to an integer", str);
                            }
                        }
                        if (parameters.ContainsKey(PushNotificationKey.Tag))
                        {
                            tag = parameters[PushNotificationKey.Tag].ToString();
                        }

                        if (!parameters.ContainsKey(PushNotificationKey.Silent) || !System.Boolean.Parse(parameters[PushNotificationKey.Silent].ToString()))
                        {
                            if (CrossPushNotification.PushNotificationListener.ShouldShowNotification())
                            {
                                CreateNotification(title, message, notifyId, tag);
                            }
                        }

                    }
                    catch (Java.Lang.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                    catch (System.Exception ex1)
                    {
                        System.Diagnostics.Debug.WriteLine(ex1.ToString());
                    }
                


            }
        }
     

        void CreateNotification(string title, string message, int notifyId, string tag)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - PushNotification - Message {1} : {2}", PushNotificationKey.DomainName,title,message));

            NotificationCompat.Builder builder = null;
            Context context = Android.App.Application.Context;

            if (CrossPushNotification.SoundUri == null)
            {
                CrossPushNotification.SoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            }
            try
            {

                if (CrossPushNotification.IconResource == 0)
                {
                    CrossPushNotification.IconResource = context.ApplicationInfo.Icon;
                }
                else
                {
                    string name = context.Resources.GetResourceName(CrossPushNotification.IconResource);

                    if (name == null)
                    {
                        CrossPushNotification.IconResource = context.ApplicationInfo.Icon;

                    }
                }

            }
            catch (Android.Content.Res.Resources.NotFoundException ex)
            {
                CrossPushNotification.IconResource = context.ApplicationInfo.Icon;
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
                      .SetSound(CrossPushNotification.SoundUri)
                      .SetSmallIcon(CrossPushNotification.IconResource) // This is the icon to display
                      .SetContentText(message); // the message to display.

            NotificationManager notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            notificationManager.Notify(tag, notifyId, builder.Build());

        }
    }
}
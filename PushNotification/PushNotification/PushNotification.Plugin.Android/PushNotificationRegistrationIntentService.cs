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
using Android.Gms.Iid;
using Android.Gms.Gcm;
using PushNotification.Plugin.Abstractions;
using Android.Support.V4.Content;

namespace PushNotification.Plugin
{
    [Service(Exported = false)]
    public class PushNotificationRegistrationIntentService : IntentService
    {

        const string Tag = "PushNotificationRegistationIntentService";
        private string[] Topics = new string[] { "global" };
        private readonly object syncLock = new object();

        protected override void OnHandleIntent(Intent intent)
        {
            try
            {
                Bundle extras = intent.Extras;
                lock (syncLock)
                {
                    InstanceID instanceID = InstanceID.GetInstance(Android.App.Application.Context);

#if _DEBUG_
                    try
                    {
                        instanceID.DeleteInstanceID();
                    }
                    catch (Exception e)
                    {
                        Android.Util.Log.Debug(Tag, e.ToString());
                    }
                    instanceID = InstanceID.GetInstance(Android.App.Application.Context);
#endif

                    string token = instanceID.GetToken(CrossPushNotification.SenderId,
                        GoogleCloudMessaging.InstanceIdScope, null);

                    CrossPushNotification.PushNotificationListener.OnRegistered(token, DeviceType.Android);
                    PushNotificationImplementation.StoreRegistrationId(Android.App.Application.Context, token);
                    this.SubscribeTopics(token);

                    System.Diagnostics.Debug.WriteLine($"{token} - Device registered, registration ID={Tag}");
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message} - Error : {Tag}");

                CrossPushNotification.PushNotificationListener.OnError($"{ex.ToString()} - Register - {Tag}", DeviceType.Android);


            }

            // Intent registrationComplete = new Intent(PushNotificationKey.RegistrationComplete);
            // LocalBroadcastManager.GetInstance(Android.App.Application.Context).SendBroadcast(registrationComplete);

        }

        private void SubscribeTopics(string token)
        {
            GcmPubSub pubSub = GcmPubSub.GetInstance(this);

            foreach (string topic in Topics)
            {
                pubSub.Subscribe(token, "/topics/" + topic, null);
            }
        }
    }
}
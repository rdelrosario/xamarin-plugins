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
using Android.Gms.Gcm.Iid;
using Android.Gms.Gcm;
using PushNotification.Plugin.Abstractions;
using Android.Support.V4.Content;

namespace PushNotification.Plugin
{

    [Service(Exported=false)]
    public class PushNotificationRegistrationIntentService : IntentService
    {

        const string Tag = "PushNotificationRegistationIntentService";
        private string[] Topics = new string[]{"global"};
        private readonly object syncLock = new object();

        protected override void OnHandleIntent(Intent intent)
        {
            try
            {
                Bundle extras = intent.Extras;
                lock (syncLock)
                {
                    InstanceID instanceID = InstanceID.GetInstance(Android.App.Application.Context);
                    string token = instanceID.GetToken(CrossPushNotification.SenderId,
                        GoogleCloudMessaging.InstanceIdScope, null);

                    CrossPushNotification.PushNotificationListener.OnRegistered(token, DeviceType.Android);
                    PushNotificationImplementation.StoreRegistrationId(Android.App.Application.Context, token);
                    this.SubscribeTopics(token);

                    System.Diagnostics.Debug.WriteLine(string.Format("{0} - Device registered, registration ID=" + token, Tag));
                }

            }catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0} - Error :" + ex.Message, Tag));

                CrossPushNotification.PushNotificationListener.OnError(string.Format("{0} - Register - " + ex.ToString(), Tag), DeviceType.Android);
              

            }

           // Intent registrationComplete = new Intent(PushNotificationKey.RegistrationComplete);
            // LocalBroadcastManager.GetInstance(Android.App.Application.Context).SendBroadcast(registrationComplete);

        }

        private void SubscribeTopics(string token)
        {
            GcmPubSub pubSub = GcmPubSub.GetInstance(this);

            foreach(string topic in Topics) 
            {
                 pubSub.Subscribe(token, "/topics/" + topic, null);
            }
        }
    }
}
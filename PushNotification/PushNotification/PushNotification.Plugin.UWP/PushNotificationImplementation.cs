﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushNotification.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.PushNotifications;

namespace PushNotification.Plugin
{
    public class PushNotificationImplementation : IPushNotification
    {

        private PushNotificationChannel channel;
        private JsonSerializer serializer = new JsonSerializer() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public string Token
        {
            get
            {
                if (channel != null)
                {
                    return channel.Uri.ToString();
                }
                else
                {
                    return null;
                }
            }
        }

        public void Register()
        {

            Debug.WriteLine("Creating Push Notification Channel For Application");

            Task<PushNotificationChannel> channelTask = PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync().AsTask();

            channelTask.Wait();

            Debug.WriteLine("Creating Push Notification Channel For Application - Done");

            channel = channelTask.Result;

            Debug.WriteLine("Registering call back for Push Notification Channel");

            channel.PushNotificationReceived += Channel_PushNotificationReceived;

            CrossPushNotification.PushNotificationListener.OnRegistered(Token, DeviceType.Windows);

        }

        private void Channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {

            Debug.WriteLine("Push Notification Received " + args.NotificationType);

            JObject jobject = null;

            switch (args.NotificationType)
            {

                case PushNotificationType.Badge:
                    jobject = JObject.FromObject(args.BadgeNotification, serializer);
                    break;

                case PushNotificationType.Raw:
                    jobject = JObject.FromObject(args.RawNotification, serializer);
                    break;

                case PushNotificationType.Tile:
                    jobject = JObject.FromObject(args.TileNotification, serializer);
                    break;
                #if WINDOWS_UWP || WINDOWS_PHONE_APP
                case PushNotificationType.TileFlyout:
                    jobject = JObject.FromObject(args.TileNotification, serializer);
                    break;
                #endif
                case PushNotificationType.Toast:
                    jobject = JObject.FromObject(args.ToastNotification, serializer);
                    break;

            }

            Debug.WriteLine("Sending JObject to PushNotificationListener " + args.NotificationType);

            CrossPushNotification.PushNotificationListener.OnMessage(jobject, DeviceType.Windows);


        }

        public void Unregister()
        {
            if(channel!=null)
            {

            channel.PushNotificationReceived -= Channel_PushNotificationReceived;
            channel = null;

            }

            CrossPushNotification.PushNotificationListener.OnUnregistered(DeviceType.Windows);

        }
    }
}

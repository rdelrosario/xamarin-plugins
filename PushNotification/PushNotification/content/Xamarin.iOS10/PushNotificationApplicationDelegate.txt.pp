using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if __UNIFIED__
using Foundation;
using UIKit;
#else
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif
using PushNotification.Plugin.Abstractions;
using PushNotification.Plugin;

namespace $rootnamespace$
{
    /*
      This class is to handle push notifications on iOS so that can be delegated to a class that implements IPushNotificationListener
      You can just add the methods of this class tou your AppDelegate or use this class. But if you use this class you must rename the class name and inheritance to the same name of your AppDelegate class and the same class inherits from, so that it can work correctly as the AppDelegate partial class.
      You must include CrossPushNotification initialization on your AppDelegate FinishedLaunching method. 
     
      Example:

        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			//...
			CrossPushNotification.Initialize<CrossPushNotificationListener> ();
                        //...
			return base.FinishedLaunching (app, options);
		}
 
    */
	public partial class PushNotificationApplicationDelegate : UIApplicationDelegate
    {


        const string TAG = "PushNotification-APN";

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
        

			if (CrossPushNotification.Current is IPushNotificationHandler) 
			{
				((IPushNotificationHandler)CrossPushNotification.Current).OnErrorReceived(error);

			}

        
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
			if (CrossPushNotification.Current is IPushNotificationHandler) 
			{
				((IPushNotificationHandler)CrossPushNotification.Current).OnRegisteredSuccess(deviceToken);

			}

        }

        public override void DidRegisterUserNotificationSettings(UIApplication application, UIUserNotificationSettings notificationSettings)
        {
            application.RegisterForRemoteNotifications();
        }

        /* Uncomment if using remote background notifications. To support this background mode, enable the Remote notifications option from the Background modes section of iOS project properties. (You can also enable this support by including the UIBackgroundModes key with the remote-notification value in your app�s Info.plist file.)
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
			if (CrossPushNotification.Current is IPushNotificationHandler) 
			{
				((IPushNotificationHandler)CrossPushNotification.Current).OnMessageReceived(userInfo);

			}
        }
        */

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
     
			if (CrossPushNotification.Current is IPushNotificationHandler) 
			{
				((IPushNotificationHandler)CrossPushNotification.Current).OnMessageReceived(userInfo);

			}
        }
    }
}
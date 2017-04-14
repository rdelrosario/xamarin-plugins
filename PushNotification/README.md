## Push Notification Plugin for Xamarin

Simple cross platform plugin to handle push notification events such as registering, unregistering and messages arrival on Android, iOS, UWP platforms.

### Setup
* Available on NuGet: http://www.nuget.org/packages/Xam.Plugin.PushNotification
* Install into your PCL project and Client projects.

**Platform Support**

|Platform|Supported|Version|
| ------------------- | :-----------: | :------------------: |
|Xamarin.iOS Unified|Yes|iOS 7+|
|Xamarin.Android|Yes|API 16+|
|Windows 10 UWP|Yes|10+|

### TODO
* Custom Notification Styling
* Badges control support
* Custom Notification processing handlers
* FCM support
* Push notification video tutorial
* Handling Notification click event

### API Usage

Call **CrossPushNotification.Current** from any project or PCL to gain access to APIs.

Must initialize plugin on each platform before use. If not initializated before using a method (Register/Unregister) will thow <b>PushNotificationNotInitializedException</b>.

**CrossPushNotification.Initialize<'T'>**
This methods initializes push notification plugin. The generic <b>T</b> should be a class that implements IPushNotificationListener. This will be the class were you would listen to all push notifications events.

####iOS
 On the AppDelegate:
```
public override bool FinishedLaunching (UIApplication app, NSDictionary options)
{
	//Consider inizializing before application initialization, if using any CrossPushNotification method during application initialization.
	   CrossPushNotification.Initialize<CrossPushNotificationListener> ();
    //...
	return base.FinishedLaunching (app, options);
}
```

####Android
 On the Android Application class, is better to use the OnCreate of the Android Application class so you can handle push notifications even when activities are closed or app not running by using our PushNotificationService to keep listening to push notifications.

But is that not a requirement you could do initialization on your MainActivity class.
```
public override void OnCreate()
{
	base.OnCreate();

	 //...
                        
    //Should specify android Sender Id as parameter 
    CrossPushNotification.Initialize<CrossPushNotificationListener>("<ANDROID SENDER ID>");            
    //...
}
```

**IPushNotificationListener implementation**

Must implement IPushNotificationListener. This would be commonly implemented in the Core project if sharing code between Android or iOS. In the case you are using the plugin only for a specific platform this would be implemented in that platform.

```
using Newtonsoft.Json.Linq;
using PushNotification.Plugin;
using PushNotification.Plugin.Abstractions;

....

public class  CrossPushNotificationListener : IPushNotificationListener
    {
        //Here you will receive all push notification messages
        //Messages arrives as a dictionary, the device type is also sent in order to check specific keys correctly depending on the platform.
        void IPushNotificationListener.OnMessage(JObject parameters, DeviceType deviceType)
        {
            Debug.WriteLine("Message Arrived");
        }
         //Gets the registration token after push registration
        void IPushNotificationListener.OnRegistered(string Token, DeviceType deviceType)
        {
            Debug.WriteLine(string.Format("Push Notification - Device Registered - Token : {0}", Token));
	}
         //Fires when device is unregistered
        void IPushNotificationListener.OnUnregistered(DeviceType deviceType)
        {
            Debug.WriteLine("Push Notification - Device Unnregistered");
       
        }
        
        //Fires when error
        void IPushNotificationListener.OnError(string message, DeviceType deviceType)
        {
            Debug.WriteLine(string.Format("Push notification error - {0}",message));
        }
        
         //Enable/Disable Showing the notification
        bool IPushNotificationListener.ShouldShowNotification()
        {
            return true;
        }
    }
```


Enum of Device Types:

```
/// <summary>
/// Device type.
/// </summary>
     public enum DeviceType
    {
        Android,
        iOS,
        WindowsPhone,
        Windows
    }
```

**Register**

Register device to receive push notifications. Should only be used after initialization, if not will get <b>PushNotificationNotInitializedException</b>.

```
void Register();
```

**Unregister**

Unregister device to stop receiving push notifications. Should only be used after initialization, if not will get <b>PushNotificationNotInitializedException</b>.

```
void Unregister();
```


#### Notes

##### Android Specifics

* Must compile against 21+ as plugin is using API 21 specific things. Here is a great breakdown: http://redth.codes/such-android-api-levels-much-confuse-wow/
* The <b>package name</b> of your Android aplication must <b>start with lower case</b> or you will get the build error: <code>Installation error: INSTALL_PARSE_FAILED_MANIFEST_MALFORMED</code> 
* Make sure you have updated your Android SDK Manager libraries:

![image](https://cloud.githubusercontent.com/assets/2547751/6440604/1b0afb64-c0b5-11e4-93b8-c496e2bfa588.png)

* If you wish to receive push notitfications even when app closed. Instead of initializing the plugin in the MainActivity.cs. Do the following:

   Implement an Android Application class in your Droid project and initialize plugin there. Here a brief snippet:

   ```
   [Application]
   public class YourAndroidApplication : Application
   {
        public static Context AppContext;

        public YourAndroidApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();

            AppContext = this.ApplicationContext;

             //TODO: Initialize CrossPushNotification Plugin
             //TODO: Replace string parameter with your Android SENDER ID
             //TODO: Specify the listener class implementing IPushNotificationListener interface in the Initialize generic
             CrossPushNotification.Initialize<CrossPushNotificationListener>("<ANDROID SENDER ID>");

             //This service will keep your app receiving push even when closed.             
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
    ```
Just move your initialization stuff from MainActivity.cs to this Android Application class. Also Replace YourAndroidApplication name to your App name.


* There are a few things you can configure in Android project using the following properties from CrossPushNotification class:
    ```
    //The sets the key associated with the value will be used to show the title for the notification
    public static string NotificationContentTitleKey { get; set; }
   
    //The sets the key associated with the value will be used to show the text for the notification
    public static string NotificationContentTextKey { get; set; }
    
    //The sets the key associated with the root value will be used to show the text inside it for the notification
    public static string NotificationContentDataKey { get; set; }

    //The sets the resource id for the icon will be used for the notification
    public static int IconResource { get; set; }

    //The sets the sound  uri will be used for the notification
    public static Android.Net.Uri SoundUri { get; set; }

   ```
* By default displays a notification looking for the key <i><b>title</b></i>  to display notification title and <i><b>message</b></i>  to display notification message. If <i><b>title</b></i>  key not present will use the application name.
* If you send a key called <i><b>silent</b></i> with value true it won't display a notification just will listen to message arrival.


##### iOS Specifics
* Application should have push notification enabled and active push certificates on Apple Developer Portal.
* iOS Application Bundle identifier must be the same corresponding to the profile used for code signing the app.
* Must checkout the helper class on content folder: PushNotificationApplicationDelegate.txt. In order to setup correctly.

##### Xamarin Forms Specifics
* On Android project initialize the plugin on an Application class on android and start a sticky service so you can still receive notifications when application is closed. Consider that this application class and service can't have any Xamarin Forms related dependencies since Xamarin Forms is not initialized when app is closed, so it will crash if Xamarin dependencies are being used.

##### Additional Considerations
* On some phones android  background services might be blocked by some application. This is the case of ASUS Zenfone 3 that has an Auto-start manager, which disables background services by default. You need to make sure that your push notification service is not being blocked by some application like this one, since you won't receive push notifications when app is closed if so.

#### Contributors
* [rdelrosario](https://github.com/rdelrosario)
* [aflorenzan](https://github.com/aflorenzan)
* [totemika](https://github.com/totemika)
* [howbazaar](https://github.com/howbazaar)
* [jmeadecvlt](https://github.com/jmeadecvlt)
* [cupn](https://github.com/cupn)
* [charri](https://github.com/charri)
* [timbrand](https://github.com/timbrand)
* [kentcb](https://github.com/kentcb)
* [Alessandro Moscatelli](https://www.linkedin.com/in/alessandromoscatelli)
* [havalli](https://github.com/havalli)
* [cschwarz](https://github.com/cschwarz)
* [rhishikeshj](https://github.com/rhishikeshj)

Thanks!

#### License
Licensed under main repo license

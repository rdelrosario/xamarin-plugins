## Push Notification Plugin for Xamarin

Simple cross platform plugin to handle push notification events such as registering, unregistering and messages arrival on Android and iOS.

### Setup
* Available on NuGet: http://www.nuget.org/packages/Xam.Plugin.PushNotification
* Install into your PCL project and Client projects.

**Supports**
* Xamarin.iOS
* Xamarin.iOS (x64 Unified)
* Xamarin.Android

### TODO
* Include Windows Phone 8 (Silverlight), Windows Phone 8.1 RT, Windows Store 8.1 Support

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
public class  CrossPushNotificationListener : IPushNotificationListener
    {
        //Here you will receive all push notification messages
        //Messages arrives as a dictionary, the device type is also sent in order to check specific keys correctly depending on the platform.
        void IPushNotificationListener.OnMessage(IDictionary<string, object> Parameters, DeviceType deviceType)
        {
            Debug.WriteLine("Message Arrived");
        }
         //Gets the registration token after push registration
        void IPushNotificationListener.OnRegistered(string Token, DeviceType deviceType)
        {
            Debug.WriteLine(string.Format("Push Notification - Device Registered - Token : {0}", Token));

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
        iOS
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

There are a few things you can configure in Android project using the following properties from CrossPushNotification class:
```
    //The sets the key associated with the value will be used to show the title for the notification
    public static string NotificationContentTitleKey { get; set; }
   
    ////The sets the key associated with the value will be used to show the text for the notification
    public static string NotificationContentTextKey { get; set; }

    //The sets the resource id for the icon will be used for the notification
    public static int IconResource { get; set; }

    //The sets the sound  uri will be used for the notification
    public static Android.Net.Uri SoundUri { get; set; }

```
* By default displays a notification looking for the key <i><b>title</b></i>  to display notification title and <i><b>message</b></i>  to display notification message. If <i><b>title</b></i>  key not present will use the application name.
* If you send a key called <i><b>silent</b></i> with value true it won't display a notification just will listen to message arrival.
* The <b>package name</b> of your Android aplication must <b>start with lower case</b> or you will get the build error: <code>Installation error: INSTALL_PARSE_FAILED_MANIFEST_MALFORMED</code> 
* Make sure you have updated your Android SDK Manager libraries:

![image](https://cloud.githubusercontent.com/assets/2547751/6440604/1b0afb64-c0b5-11e4-93b8-c496e2bfa588.png)


##### iOS Specifics
* Application should have push notification enabled and active push certificates on Apple Developer Portal.
* iOS Application Bundle identifier must be the same corresponding to the profile used for code signing the app.
* Must checkout the helper class on content folder: PushNotificationApplicationDelegate.txt. In order to setup correctly.



#### Contributors
* [rdelrosario](https://github.com/rdelrosario)
* [aflorenzan](https://github.com/aflorenzan)
* [totemika](https://github.com/totemika)

Thanks!

#### License
Licensed under main repo license

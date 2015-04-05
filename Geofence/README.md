## Geofence Plugin for Xamarin

Simple cross platform plugin to handle geofence events such as entering, leaving and staying in a geofence region.

### Setup
* Available on NuGet: http://www.nuget.org/packages/Xam.Plugin.Geofence
* Install into your PCL project and Client projects.

**Supports**
* Xamarin.iOS
* Xamarin.iOS (x64 Unified)
* Xamarin.Android

### TODO
* Include Windows Phone 8 (Silverlight), Windows Phone 8.1 RT, Windows Store 8.1 Support

### API Usage

Call **CrossGeofence.Current** from any project or PCL to gain access to APIs.

Must initialize plugin on each platform before use. If not initializated before using a method (Register/Unregister) will thow <b>GeofenceNotInitializedException</b>.

**CrossGeofence.Initialize<'T'>**
This methods initializes geofence plugin. The generic <b>T</b> should be a class that implements IGeofenceListener. This will be the class were you would listen to all geofence events.

####iOS
 On the AppDelegate:
```
public override bool FinishedLaunching (UIApplication app, NSDictionary options)
{
	//Consider inizializing before application initialization, if using any CrossGeofence method during application initialization.

	//Initialization...

    CrossGeofence.Initialize<CrossGeofenceListener> ();

	return base.FinishedLaunching (app, options);
}
```

####Android
 On the Android Application class, is better to use the OnCreate of the Android Application class so you can handle geofence events even when activities are closed or app not running by using our GeofenceService to keep listening to geofence events.

Initialization on your MainActivity/Application class.
```
public override void OnCreate()
{
	base.OnCreate();

	 //...
                        
    CrossGeofence.Initialize<CrossGeofenceListener>();            
    //...
}
```

**IGeofenceListener implementation**

Must implement IGeofenceListener. This would be commonly implemented in the Core project if sharing code between Android or iOS. In the case you are using the plugin only for a specific platform this would be implemented in that platform.

```
public class  CrossGeofenceListener : IGeofenceListener
    {
         public void OnMonitoringStarted()
        {
            Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Tag, "Monitoring started in all regions"));
        }

        public void OnMonitoringStopped()
        {
            Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Tag, "Monitoring stopped for all regions"));
        }

        public void OnMonitoringStopped(string identifier)
        {
            Debug.WriteLine(string.Format("{0} - {1}: {2}", CrossGeofence.Tag, "Monitoring stopped in region", identifier));
        }

        public void OnError(string error)
        {
            Debug.WriteLine(string.Format("{0} - {1}: {2}", CrossGeofence.Tag, "Error", error));
        }


        public void OnRegionStateChanged(GeofenceResult result)
        {
            Debug.WriteLine(string.Format("{0} - {1}", CrossGeofence.Tag, result.ToString()));
        }
```


Enum of Transaction Types:

```
/// <summary>
/// Transaction type.
/// </summary>
     public enum TransactionType
    {
        Entered,
        Exited,
        Stayed
    }
```

**StartMonitoring**

Start monitoring regions to listen to events. Should only be used after initialization, if not will get <b>GeofenceNotInitializedException</b>.

```
void StartMonitoring(IList<GeofenceCircularRegion> regions);
```

**StopMonitoring**

Stop monitoring all geofence regions. Should only be used after initialization, if not will get <b>GeofenceNotInitializedException</b>.

```
void StopMonitoring();
```


#### Notes

##### Android Specifics

There are a few things you can configure in Android project using the following properties from CrossGeofence class:
```

    //The sets the resource id for the icon will be used for the notification
    public static int IconResource { get; set; }

    //The sets the sound  uri will be used for the notification
    public static Android.Net.Uri SoundUri { get; set; }

```
* Requires *android.permission.ACCESS_FINE_LOCATION* && *com.google.android.providers.gsf.permission.READ_GSERVICES* permissions.
* The <b>package name</b> of your Android aplication must <b>start with lower case</b> or you will get the build error: <code>Installation error: INSTALL_PARSE_FAILED_MANIFEST_MALFORMED</code> 
* Make sure you have updated your Android SDK Manager libraries:

![image](https://cloud.githubusercontent.com/assets/2547751/6440604/1b0afb64-c0b5-11e4-93b8-c496e2bfa588.png)


##### iOS Specifics

* Must checkout the helper class on content folder: GeofenceReadme.txt. In order to setup correctly.



#### Contributors
* [rdelrosario](https://github.com/rdelrosario)
* [aflorenzan](https://github.com/aflorenzan)

Thanks!

#### License
Licensed under main repo license

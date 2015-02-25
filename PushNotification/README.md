## Push Notification Plugin for Xamarin

Simple cross platform plugin to register, unregister and receive push notifications messages on Android and iOS.

### Setup
* Comming soon to NuGet

**Supports**
* Xamarin.iOS
* Xamarin.iOS (x64 Unified)
* Xamarin.Android

### API Usage

Call **CrossPushNotification.Current** from any project or PCL to gain access to APIs.


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

Register device to receive push notifications
```
/// <param name="id">Android Sender ID</param>

void Register(string id = null);
```

**Unregister**

Unregister device to stop receiving push notifications
```
void Unregister(string id = null);
```

#### Events

You can subscribe to <b>OnRegistered</b>, which will return <b>PushNotificationRegistrationEventArgs</b> with the push token and the device type was registered.

```
/// <summary>
/// Occurs when device gets registered for push notifications.
/// </summary>
event PushNotificationRegistrationEventHandler OnRegistered;
```
**PushNotificationRegistrationEventArgs Properties**
```
/// <summary>
/// Device Push token.
/// </summary>
/// <value>The push token of device.</value>
public string Token;
/// <summary>
/// Type of device registered
/// </summary>
/// <value>device type</value>
public DeviceType DeviceType;
```

```
/// <summary>
/// Occurs when device gets unegistered from push notifications.
/// </summary>
event PushNotificationUnregistrationEventHandler OnUnregistered;
```
**PushNotificationBaseEventArgs Properties**
```
/// <summary>
/// Type of device registered
/// </summary>
/// <value>device type</value>
public DeviceType DeviceType;
```

**Push Registration Example**
```
	CrossPushNotification.Current.OnRegistered += (s, e) => 
	{
		System.Console.WriteLine(e.Token);
	};

    //Android GCM Sender ID Specified as parameter
	CrossPushNotification.Current.Register("346858015989");

```

**Push Unregistration Example**
```
	CrossPushNotification.Current.OnUnregistered += (s, e) => 
	{
		System.Console.WriteLine(e.DeviceType);
	};

	CrossPushNotification.Current.Unregister();

```

**Push On Message Arrival Example**
```
	CrossPushNotification.Current.OnMessage += (s, e) => 
	{
            foreach(var p in e.Parameters.Keys)
					System.Diagnostics.Debug.WriteLine("{0} - {1}",p,e.Parameters[p]);

	};

```

#### Notes


#### Contributors
* [rdelrosario](https://github.com/rdelrosario)

Thanks!

#### License
Licensed under main repo license
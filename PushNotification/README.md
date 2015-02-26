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

Register device to receive push notifications

```
void Register();
```

**Unregister**

Unregister device to stop receiving push notifications

```
void Unregister();
```


#### Notes


#### Contributors
* [rdelrosario](https://github.com/rdelrosario)

Thanks!

#### License
Licensed under main repo license
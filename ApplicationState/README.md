## Application State Plugin for Xamarin

Simple cross platform plugin to check if application is on foreground or background. 

**Supports**
* Xamarin.iOS
* Xamarin.iOS (x64 Unified)
* Xamarin.Android

**TODO: Windows Platforms Support**
* Windows Phone 8 (Silverlight)
* Windows Phone 8.1 RT
* Windows Store 8.1

### API Usage

Call **CrossApplicationState.Current** from any project or PCL to gain access to APIs.

**IsForeground**
```
/// <summary>
/// Determines if the application is on foreground
/// </summary>
bool IsForeground;
```

Returns true if application state is foreground false if not.

**IsBackground**
```
/// <summary>
/// Determines if the application is on background.
/// </summary>
bool IsBackground;
```

Returns true if application state is background false if not.


#### Contributors
* [rdelrosario](https://github.com/rdelrosario)

Thanks!

#### License
Licensed under main repo license
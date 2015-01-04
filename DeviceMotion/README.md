## Device Motion Plugin for Xamarin and Windows

Simple cross platform plugin to read motion vectors value  for device motion sensors such as:  Accelerometer, Gyroscope, Magnetometer. 

### Setup
* Available on NuGet: http://www.nuget.org/packages/Xam.Plugin.DeviceMotion
* Install into your PCL project and Client projects.

**Supports**
* Xamarin.iOS
* Xamarin.iOS (x64 Unified)
* Xamarin.Android
* Windows Phone 8 (Silverlight)
* Windows Phone 8.1 RT


### API Usage

Call **CrossDeviceMotion.Current** from any project or PCL to gain access to APIs.

Enum of Device Motion Sensor Type:

```
/// <summary>
/// Motion sensor type enum. 
/// </summary>
	public enum MotionSensorType
	{
		/// <summary>
		/// The accelerometer.
		/// </summary>
		Accelerometer,
		/// <summary>
		/// The gyroscope.
		/// </summary>
		Gyroscope,
		/// <summary>
		/// The magnetometer.
		/// </summary>
		Magnetometer,

	}
```
Enum of Device Motion Sensor Delay:

```
/// <summary>
/// Motion sensor delay enum.(Specified in milliseconds)
/// </summary>
	public enum MotionSensorDelay 
	{
		/// <summary>
		/// The fastest.
		/// </summary>
		Fastest = 0,
		/// <summary>
		/// The game.
		/// </summary>
		Game = 20,
		/// <summary>
		/// The user interface.
		/// </summary>
		Ui = 60,
		/// <summary>
		/// The default.
		/// </summary>
		Default = 200

	}
```

**Start**

Starts sensor reading for the specified sensor type and sensor delay interval.
```
/// <summary>
/// Start reading for the specified sensorType (Motion Sensor Type Enum Value) with an update delay interval (Motion Sensor Delay Enum Value) .
/// </summary>
/// <param name="sensorType">Sensor type.</param>
/// <param name="interval">Interval.</param>

void Start(MotionSensorType sensorType,MotionSensorDelay interval);
```

**Stop**

Stops sensor reading for the specified sensor type.
```
/// <summary>
/// Stop reading for the specified sensorType. (Motion Sensor Type Enum Value)
/// </summary>
/// <param name="sensorType">Sensor type.</param>
void Stop(MotionSensorType sensorType);
```

**IsActive**
```
/// <summary>
/// Determines whether the specified sensorType (Motion Sensor Type Enum Value) is active or not.
/// </summary>
/// <param name="sensorType">Sensor type.</param>
bool IsActive(MotionSensorType sensorType);
```

Returns true if specified sensor type is active, false if not.

#### Events

You can subscribe to <b>SensorValueChanged</b>, which will return <b>SensorValueChangedEventArgs</b> with all information you need. This occurs when a new sensor reading is available at the specified interval.

```
/// <summary>
/// Occurs when sensor value changed.
/// </summary>
event SensorValueChangedEventHandler SensorValueChanged;
```
**SensorValueChangedEventArgs Properties**
```
/// <summary>
/// Type of the sensor.
/// </summary>
/// <value>The type of the sensor.</value>
public MotionSensorType SensorType;
/// <summary>
/// Motion Vector provide X,Y,Z values
/// </summary>
/// <value>Vector Value</value>
public MotionVector Value;
```

Note: Magnetometer API is not available for Windows Store & Windows Phone 8.

#### Contributors
* [rdelrosario](https://github.com/rdelrosario)

Thanks!

#### License
Licensed under main repo license
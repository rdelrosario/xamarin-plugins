## Device Motion Plugin for Xamarin and Windows

Simple cross platform plugin to read motion vectors value for device motion sensors such as:  Accelerometer, Gyroscope, Magnetometer, Compass. 

### Setup
* Available on NuGet: http://www.nuget.org/packages/Xam.Plugin.DeviceMotion
* Install into your PCL project and Client projects.

**Supports**
* Xamarin.iOS
* Xamarin.iOS (x64 Unified)
* Xamarin.Android
* Windows Phone 8 (Silverlight)
* Windows Phone 8.1 RT
* Windows Store 8.1

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
        /// Accelerometer Sensor
        /// </summary>
		Accelerometer,
        /// <summary>
        /// Gyroscope Sensor
        /// </summary>
		Gyroscope,
        /// <summary>
        /// Magnetometer Sensor
        /// </summary>
		Magnetometer,
        /// <summary>
        /// Compass Sensor
        /// </summary>
        Compass

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
Enum of Device Motion Sensor Value Type:

```
/// <summary>
/// Motion sensor value type.
/// </summary>
    public enum MotionSensorValueType
    {
		/// <summary>
		/// Single value. 
		/// </summary>
        Single,
		/// <summary>
		/// Vector value.
		/// </summary>
        Vector
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
/// Motion Value
/// </summary>
/// <value>Value/value>
public MotionValue Value;
/// <summary>
/// Gets the type of the value. Indicates if sensor value is single value or a vector value
/// </summary>
/// <value>The type of the value.</value>
public MotionSensorValueType ValueType
```
If <b>ValueType</b> is <b>MotionSensorValueType.Vector</b> cast the <b>Value</b> to <b>MotionVector</b> to get the X,Y,Z Vector values. If <b>ValueType</b> is <b>MotionSensorValueType.Single</b> cast it to <b>MotionValue</b> and use the <b>Value</b> property of the <b>MotionValue</b> to get the sensor measure. For Compass sensor returns single value other sensors returns vector value

**Reading Sensor Changes Example**
```
CrossDeviceMotion.Current.SensorValueChanged+=(s, a)=>{
		
				switch(a.SensorType){
				   case MotionSensorType.Accelerometer:
					   Debug.WriteLine("A: {0},{1},{2}",((MotionVector)a.Value).X,((MotionVector)a.Value).Y,((MotionVector)a.Value).Z);
					break;
				    case MotionSensorType.Compass:
					   Debug.WriteLine("H: {0}",a.Value);
					break;
				}
};
```
#### Notes

* Magnetometer API is not available for Windows Store & Windows Phone 8 (Silverlight). 
* On iOS for Compass sensor interval parameter is ignored for the moment.

#### Contributors
* [rdelrosario](https://github.com/rdelrosario)

Thanks!

#### License
Licensed under main repo license
using DeviceMotion.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tizen.Sensor;

namespace DeviceMotion.Plugin
{
	/// <summary>
	/// Implementation for DeviceMotion
	/// </summary>
	/// <summary>
	/// Device motion implementation.
	/// </summary>
	public class DeviceMotionImplementation : IDeviceMotion
	{
		private Accelerometer accelerometer;
		private Gyroscope gyroscope;
		private Magnetometer magnetometer;
		private OrientationSensor orientation;

		private double ms = 1000.0;
		private IDictionary<MotionSensorType, bool> sensorStatus;
		private static List<Sensor> sensors = new List<Sensor>();

		/// <summary>
		/// Initializes a new instance of the DeviceMotionImplementation class.
		/// </summary>
		public DeviceMotionImplementation()
		{
			if (Accelerometer.IsSupported)
			{
				accelerometer = new Accelerometer();
				sensors.Add(accelerometer);
			}				
			if (Gyroscope.IsSupported)
			{
				gyroscope = new Gyroscope();
				sensors.Add(gyroscope);
			}
			if (Magnetometer.IsSupported)
			{
				magnetometer = new Magnetometer();
				sensors.Add(magnetometer);
			}
			if (OrientationSensor.IsSupported)
			{
				orientation = new OrientationSensor();
				sensors.Add(orientation);
			}

			sensorStatus = new Dictionary<MotionSensorType, bool>(){
				{ MotionSensorType.Accelerometer, false },
				{ MotionSensorType.Gyroscope, false },
				{ MotionSensorType.Magnetometer, false },
				{ MotionSensorType.Compass, false }
			};
		}

		/// <summary>
		/// Occurs when sensor value changed.
		/// </summary>
		public event SensorValueChangedEventHandler SensorValueChanged;

		/// <summary>
		/// Start the specified sensorType and interval.
		/// </summary>
		/// <param name="sensorType">Sensor type.</param>
		/// <param name="interval">Interval.</param>
		public void Start(MotionSensorType sensorType, MotionSensorDelay interval = MotionSensorDelay.Default)
		{
			uint delay = (uint)((double)Convert.ToInt32(interval) / ms);
			switch (sensorType)
			{
				case MotionSensorType.Accelerometer:
					if (accelerometer != null)
					{
						accelerometer.Interval = delay;
						accelerometer.DataUpdated += AccelerometerDataUpdated;
						accelerometer.Start();
						sensorStatus[sensorType] = true;
					}
					else
					{
						Debug.WriteLine("Accelerometer not available");
					}
					break;
				case MotionSensorType.Gyroscope:
					if (gyroscope != null)
					{
						gyroscope.Interval = delay;
						gyroscope.DataUpdated += GyroscopeDataUpdated;
						gyroscope.Start();
						sensorStatus[sensorType] = true;
					}
					else
					{
						Debug.WriteLine("Gyroscope not available");
					}
					break;
				case MotionSensorType.Magnetometer:
					if(magnetometer != null)
					{
						magnetometer.Interval = delay;
						magnetometer.DataUpdated += MagnetometerDataUpdated;
						magnetometer.Start();
						sensorStatus[sensorType] = true;
					}
					else
					{
						Debug.WriteLine("Magnetometer not available");
					}
					break;
				case MotionSensorType.Compass:

					if (orientation != null)
					{
						orientation.Interval = delay;
						orientation.DataUpdated += OrientationDataUpdated;
						orientation.Start();
						sensorStatus[sensorType] = true;
					}
					else
					{
						Debug.WriteLine("OrientationSensor not available");
					}
					break;
			}
		}

		/// <summary>
		/// Stop the specified sensorType.
		/// </summary>
		/// <param name="sensorType">Sensor type.</param>
		public void Stop(MotionSensorType sensorType)
		{
			switch (sensorType)
			{
				case MotionSensorType.Accelerometer:
					if (accelerometer != null)
					{
						accelerometer.DataUpdated -= AccelerometerDataUpdated;
						accelerometer.Stop();
					}
					else
					{
						Debug.WriteLine("Accelerometer not available");
					}
					break;
				case MotionSensorType.Gyroscope:
					if (gyroscope != null)
					{
						gyroscope.DataUpdated -= GyroscopeDataUpdated;
						gyroscope.Stop();
					}
					else
					{
						Debug.WriteLine("Gyrometer not available");
					}
					break;
				case MotionSensorType.Magnetometer:
					if (magnetometer!=null)
					{
						magnetometer.DataUpdated -= MagnetometerDataUpdated;
						magnetometer.Stop();
					}
					else
					{
						Debug.WriteLine("Magnetometer not available");
					}
					break;
				case MotionSensorType.Compass:
					if (orientation != null)
					{
						orientation.DataUpdated -= OrientationDataUpdated;
						orientation.Stop();
					}
					else
					{
						Debug.WriteLine("OrientationSensor not available");
					}
					break;
			}
			sensorStatus[sensorType] = false;
		}

		private void MagnetometerDataUpdated(object sender, MagnetometerData​Updated​Event​Args args)
		{
			SensorValueChanged?.Invoke(this, new SensorValueChangedEventArgs { ValueType = MotionSensorValueType.Vector, SensorType = MotionSensorType.Magnetometer, Value = new MotionVector() { X = args.X, Y = args.Y, Z = args.Z } });
		}

		private void OrientationDataUpdated(object sender, OrientationSensorData​Updated​Event​Args args)
		{
			SensorValueChanged?.Invoke(this, new SensorValueChangedEventArgs { ValueType = MotionSensorValueType.Single, SensorType = MotionSensorType.Compass, Value = new MotionValue() { Value = args.Azimuth } });
		}

		private void GyroscopeDataUpdated(object sender, GyroscopeData​Updated​Event​Args args)
		{
			SensorValueChanged?.Invoke(this, new SensorValueChangedEventArgs { ValueType = MotionSensorValueType.Vector, SensorType = MotionSensorType.Gyroscope, Value = new MotionVector() { X = args.X, Y = args.Y, Z = args.Z } });
		}

		private void AccelerometerDataUpdated(object sender, Accelerometer​Data​Updated​Event​Args args)
		{
			SensorValueChanged?.Invoke(this, new SensorValueChangedEventArgs { ValueType = MotionSensorValueType.Vector, SensorType = MotionSensorType.Accelerometer, Value = new MotionVector() { X = args.X, Y = args.Y, Z = args.Z } });
		}

		/// <summary>
		/// Determines whether this instance is active the specified sensorType.
		/// </summary>
		/// <returns><c>true</c> if this instance is active the specified sensorType; otherwise, <c>false</c>.</returns>
		/// <param name="sensorType">Sensor type.</param>
		public bool IsActive(MotionSensorType sensorType)
		{
			return sensorStatus[sensorType];
		}

		#region IDisposable implementation
		/// <summary>
		/// Dispose of class and parent classes
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose up
		/// </summary>
		~DeviceMotionImplementation()
		{
			Dispose(false);
		}
		private bool disposed = false;
		/// <summary>
		/// Dispose method
		/// </summary>
		/// <param name="disposing"></param>
		public virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					//dispose only
					foreach (var item in sensors)
					{
						if (item != null)
							item.Dispose();
					}
					sensors.Clear();
				}

				disposed = true;
			}
		}
		#endregion
	}
}

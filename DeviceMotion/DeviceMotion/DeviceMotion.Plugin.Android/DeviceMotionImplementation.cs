using DeviceMotion.Plugin.Abstractions;
using Android.Hardware;
using Android.Content;
using Android.App;
using Android.Runtime;
using System;
using System.Collections.Generic;


namespace DeviceMotion.Plugin
{
  /// <summary>
  /// Implementation for Feature
  /// </summary>
  public class DeviceMotionImplementation : Java.Lang.Object , ISensorEventListener, IDeviceMotion
  {
      private SensorManager sensorManager;
      private Sensor sensorAccelerometer;
      private Sensor sensorGyroscope;
      private Sensor sensorMagnetometer;

      private IDictionary<MotionSensorType, bool> sensorStatus;

      /// <summary>
      /// Initializes a new instance of the DeviceMotionImplementation class.
      /// </summary>
      public DeviceMotionImplementation() : base()
      {

          sensorManager = (SensorManager)Application.Context.GetSystemService(Context.SensorService);
          sensorAccelerometer = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
          sensorGyroscope = sensorManager.GetDefaultSensor(SensorType.Gyroscope);
          sensorMagnetometer = sensorManager.GetDefaultSensor(SensorType.MagneticField);

          sensorStatus = new Dictionary<MotionSensorType, bool>(){
				{ MotionSensorType.Accelerometer, false},
				{ MotionSensorType.Gyroscope, false},
				{ MotionSensorType.Magnetometer, false}
			};
      }

      /// <summary>
      /// Occurs when sensor value changed.
      /// </summary>
      public event SensorValueChangedEventHandler SensorValueChanged;

      /// <Docs>To be added.</Docs>
      /// <summary>
      /// Called when the accuracy of a sensor has changed.
      /// </summary>
      /// <para tool="javadoc-to-mdoc">Called when the accuracy of a sensor has changed.</para>
      /// <param name="sensor">Sensor.</param>
      /// <param name="accuracy">Accuracy.</param>
      public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
      {

      }

      /// <summary>
      /// Raises the sensor changed event.
      /// </summary>
      /// <param name="e">E.</param>
      public void OnSensorChanged(SensorEvent e)
      {
          if (SensorValueChanged == null)
              return;

          MotionSensorType mSensorType = MotionSensorType.Accelerometer;

          switch (e.Sensor.Type)
          {
              case SensorType.Accelerometer:
                  mSensorType = MotionSensorType.Accelerometer;
                  break;
              case SensorType.Gyroscope:
                  mSensorType = MotionSensorType.Gyroscope;
                  break;
              case SensorType.MagneticField:
                  mSensorType = MotionSensorType.Magnetometer;
                  break;


          }


          SensorValueChanged(this, new SensorValueChangedEventArgs { SensorType = mSensorType, Value = new MotionVector() { X = e.Values[0], Y = e.Values[1], Z = e.Values[2] } });

      }


      /// <summary>
      /// Start the specified sensorType and interval.
      /// </summary>
      /// <param name="sensorType">Sensor type.</param>
      /// <param name="interval">Interval.</param>
      public void Start(MotionSensorType sensorType, MotionSensorDelay interval = MotionSensorDelay.Default)
      {


          SensorDelay delay = SensorDelay.Normal;
          switch (interval)
          {
              case MotionSensorDelay.Fastest:
                  delay = SensorDelay.Fastest;
                  break;
              case MotionSensorDelay.Game:
                  delay = SensorDelay.Game;
                  break;
              case MotionSensorDelay.Ui:
                  delay = SensorDelay.Ui;
                  break;

          }
          switch (sensorType)
          {
              case MotionSensorType.Accelerometer:
                  if (sensorAccelerometer != null)
                      sensorManager.RegisterListener(this, sensorAccelerometer, delay);
                  else
                      Console.WriteLine("Accelerometer not available");
                  break;
              case MotionSensorType.Gyroscope:
                  if (sensorGyroscope != null)
                      sensorManager.RegisterListener(this, sensorGyroscope, delay);
                  else
                      Console.WriteLine("Gyroscope not available");
                  break;
              case MotionSensorType.Magnetometer:
                  if (sensorMagnetometer != null)
                      sensorManager.RegisterListener(this, sensorMagnetometer, delay);
                  else
                      Console.WriteLine("Magnetometer not available");
                  break;
          
          }
          sensorStatus[sensorType] = true;

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
                  if (sensorAccelerometer != null)
                      sensorManager.UnregisterListener(this, sensorAccelerometer);
                  else
                      Console.WriteLine("Accelerometer not available");
                  break;
              case MotionSensorType.Gyroscope:
                  if (sensorGyroscope != null)
                      sensorManager.UnregisterListener(this, sensorGyroscope);
                  else
                      Console.WriteLine("Gyroscope not available");
                  break;
              case MotionSensorType.Magnetometer:
                  if (sensorMagnetometer != null)
                      sensorManager.UnregisterListener(this, sensorMagnetometer);
                  else
                      Console.WriteLine("Magnetometer not available");
                  break;
          }
          sensorStatus[sensorType] = false;
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

  }
}
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
      private Sensor sensorCompass;

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
          sensorCompass = sensorManager.GetDefaultSensor(SensorType.Orientation);
          sensorStatus = new Dictionary<MotionSensorType, bool>(){
				{ MotionSensorType.Accelerometer, false},
				{ MotionSensorType.Gyroscope, false},
				{ MotionSensorType.Magnetometer, false},
                { MotionSensorType.Compass,false}
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


          switch (e.Sensor.Type)
          {
              case SensorType.Accelerometer:
				SensorValueChanged(this, new SensorValueChangedEventArgs() { ValueType = MotionSensorValueType.Vector, SensorType = MotionSensorType.Accelerometer, Value = new MotionVector() { X = e.Values[0], Y = e.Values[1], Z = e.Values[2] } });

                  break;
              case SensorType.Gyroscope:
					SensorValueChanged(this, new SensorValueChangedEventArgs() { ValueType=MotionSensorValueType.Vector,SensorType = MotionSensorType.Gyroscope, Value = new MotionVector() { X = e.Values[0], Y = e.Values[1], Z = e.Values[2] } });

                  break;
              case SensorType.MagneticField:
					SensorValueChanged(this, new SensorValueChangedEventArgs() { ValueType=MotionSensorValueType.Vector,SensorType = MotionSensorType.Magnetometer, Value = new MotionVector() { X = e.Values[0], Y = e.Values[1], Z = e.Values[2] } });

                  break;
              case SensorType.Orientation:
					SensorValueChanged(this, new SensorValueChangedEventArgs() { ValueType=MotionSensorValueType.Single,SensorType = MotionSensorType.Compass, Value = new MotionValue(){Value =e.Values[0]} });
                  break;
            
            

          }

          
         
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
              case MotionSensorType.Compass:
                  if (sensorCompass != null)
                      sensorManager.RegisterListener(this, sensorCompass, delay);
                  else
                      Console.WriteLine("Compass not available");
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
              case MotionSensorType.Compass:
                  if (sensorCompass != null)
                      sensorManager.UnregisterListener(this, sensorCompass);
                  else
                      Console.WriteLine("Compass not available");
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
using DeviceMotion.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#if __UNIFIED__
using Foundation;
using CoreMotion;
using CoreLocation;
#else
using MonoTouch.CoreMotion;
using MonoTouch.Foundation;
using MonoTouch.CoreLocation;
#endif


namespace DeviceMotion.Plugin
{
  /// <summary>
  /// Implementation for DeviceMotion
  /// </summary>
    /// <summary>
    /// Device motion implementation.
    /// </summary>
    public class DeviceMotionImplementation : IDeviceMotion, IDisposable
    {
        private double ms = 1000.0;
        private CMMotionManager motionManager;
        private CLLocationManager locationManager;
        private IDictionary<MotionSensorType, bool> sensorStatus;

        /// <summary>
        /// Initializes a new instance of the DeviceMotionImplementation class.
        /// </summary>
        public DeviceMotionImplementation()
        {
            motionManager = new CMMotionManager();
            locationManager = new CLLocationManager();
            locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
            locationManager.HeadingFilter = 1;

            sensorStatus = new Dictionary<MotionSensorType, bool>(){
				{ MotionSensorType.Accelerometer, false},
				{ MotionSensorType.Gyroscope, false},
				{ MotionSensorType.Magnetometer, false},
                { MotionSensorType.Compass, false}
			};
        }


        #region IDeviceMotion implementation
        /// <summary>
        /// Occurs when sensor value changed.
        /// </summary>
        public event SensorValueChangedEventHandler SensorValueChanged;

        /// <summary>
        /// Start the specified sensorType and interval.
        /// </summary>
        /// <param name="sensorType">Sensor type.</param>
        /// <param name="interval">Interval.</param>
        public void Start(MotionSensorType sensorType, MotionSensorDelay interval)
        {

            switch (sensorType)
            {
                case MotionSensorType.Accelerometer:
                    if (motionManager.AccelerometerAvailable)
                    {
                        motionManager.AccelerometerUpdateInterval = (double)interval / ms;
                        motionManager.StartAccelerometerUpdates(NSOperationQueue.CurrentQueue, OnAccelerometerChanged);
                    }
                    else
                    {
                      Debug.WriteLine("Accelerometer not available");
                    }
                    break;
                case MotionSensorType.Gyroscope:
                    if (motionManager.GyroAvailable)
                    {
                        motionManager.GyroUpdateInterval = (double)interval / ms;
                        motionManager.StartGyroUpdates(NSOperationQueue.CurrentQueue, OnGyroscopeChanged);
                    }
                    else
                    {
                        Debug.WriteLine("Gyroscope not available");
                    }
                    break;
                case MotionSensorType.Magnetometer:
                    if (motionManager.MagnetometerAvailable)
                    {
                        motionManager.MagnetometerUpdateInterval = (double)interval / ms;
                        motionManager.StartMagnetometerUpdates(NSOperationQueue.CurrentQueue, OnMagnometerChanged);
                    }
                    else
                    {
                        Debug.WriteLine("Magnetometer not available");
                    }
                    break;
                case MotionSensorType.Compass:
                    if (CLLocationManager.HeadingAvailable)
                    {
                        locationManager.StartUpdatingHeading();
                        locationManager.UpdatedHeading += OnHeadingChanged;
                    }
                    else
                    {
                        Debug.WriteLine("Compass not available");
                    }
                    break;
            }
            sensorStatus[sensorType] = true;
        }

        private void OnHeadingChanged(object sender, CLHeadingUpdatedEventArgs e)
        {
            if (SensorValueChanged == null)
                return;

            SensorValueChanged(this, new SensorValueChangedEventArgs { ValueType= MotionSensorValueType.Single, SensorType = MotionSensorType.Compass, Value = new MotionValue{ Value=e.NewHeading.TrueHeading }});
   
        }



        /// <summary>
        /// Raises the magnometer changed event.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="error">Error.</param>
        private void OnMagnometerChanged(CMMagnetometerData data, NSError error)
        {
            if (SensorValueChanged == null)
                return;

            SensorValueChanged(this, new SensorValueChangedEventArgs { ValueType = MotionSensorValueType.Vector, SensorType = MotionSensorType.Magnetometer, Value = new MotionVector() { X = data.MagneticField.X, Y = data.MagneticField.Y, Z = data.MagneticField.Z } });

        }

        /// <summary>
        /// Raises the accelerometer changed event.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="error">Error.</param>
        private void OnAccelerometerChanged(CMAccelerometerData data, NSError error)
        {
            if (SensorValueChanged == null)
                return;

            SensorValueChanged(this, new SensorValueChangedEventArgs { ValueType = MotionSensorValueType.Vector, SensorType = MotionSensorType.Accelerometer, Value = new MotionVector() { X = data.Acceleration.X, Y = data.Acceleration.Y, Z = data.Acceleration.Z } });

        }

        /// <summary>
        /// Raises the gyroscope changed event.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="error">Error.</param>
        private void OnGyroscopeChanged(CMGyroData data, NSError error)
        {
            if (SensorValueChanged == null)
                return;

            SensorValueChanged(this, new SensorValueChangedEventArgs { ValueType = MotionSensorValueType.Vector, SensorType = MotionSensorType.Gyroscope, Value = new MotionVector() { X = data.RotationRate.x, Y = data.RotationRate.y, Z = data.RotationRate.z } });

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
                    if (motionManager.AccelerometerActive)
                        motionManager.StopAccelerometerUpdates();
                    else
                        Debug.WriteLine("Accelerometer not available");
                    break;
                case MotionSensorType.Gyroscope:
                    if (motionManager.GyroActive)
                        motionManager.StopGyroUpdates();
                    else
                        Debug.WriteLine("Gyroscope not available");
                    break;
                case MotionSensorType.Magnetometer:
                    if (motionManager.MagnetometerActive)
                        motionManager.StopMagnetometerUpdates();
                    else
                        Debug.WriteLine("Magnetometer not available");
                    break;
                case MotionSensorType.Compass:
                    if (CLLocationManager.HeadingAvailable)
                    {
                        locationManager.StopUpdatingHeading();
                        locationManager.UpdatedHeading-= OnHeadingChanged;
                    }
                    else
                    {
                        Debug.WriteLine("Compass not available");
                    }
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

        #endregion

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
                }

                disposed = true;
            }
        }
        #endregion
    }

}
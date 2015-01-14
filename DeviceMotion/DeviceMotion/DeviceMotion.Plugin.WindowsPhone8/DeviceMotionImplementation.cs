using DeviceMotion.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.Sensors;

namespace DeviceMotion.Plugin
{
    /// <summary>
    /// Implementation for DeviceMotion
    /// </summary>
    public class DeviceMotionImplementation : IDeviceMotion
    {
        private Accelerometer accelerometer;
        private Gyrometer gyrometer;
        private Compass compass;

#if WINDOWS_PHONE_APP
        private Magnetometer magnetometer;
#endif

        private double ms = 1000.0;
        private Dictionary<MotionSensorType, bool> sensorStatus;
        /// <summary>
        /// Initializes a new instance of the DeviceMotionImplementation class.
        /// </summary>
        public DeviceMotionImplementation()
        {
            accelerometer = Accelerometer.GetDefault();
            gyrometer = Gyrometer.GetDefault();
            compass = Compass.GetDefault();

#if WINDOWS_PHONE_APP
            magnetometer = Magnetometer.GetDefault();
#endif
            sensorStatus = new Dictionary<MotionSensorType, bool>()
            {
				{ MotionSensorType.Accelerometer, false},
				{ MotionSensorType.Gyroscope, false},
				{ MotionSensorType.Magnetometer, false},
                { MotionSensorType.Compass, false}

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
                        accelerometer.ReportInterval = delay;
                        accelerometer.ReadingChanged += AccelerometerReadingChanged;
                    }
                    else
                    {
                        Debug.WriteLine("Accelerometer not available");
                    }
                    break;
                case MotionSensorType.Gyroscope:
                    if (gyrometer != null) 
                    { 
                      gyrometer.ReportInterval = delay;
                      gyrometer.ReadingChanged += GyrometerReadingChanged;
                    }
                    else
                    {
                        Debug.WriteLine("Gyrometer not available");
                    }
                    break;
                case MotionSensorType.Magnetometer:
#if WINDOWS_PHONE_APP
                    if(magnetometer != null)
                    {

                      magnetometer.ReportInterval = delay;
                      magnetometer.ReadingChanged += MagnetometerReadingChanged;
                    }
                    else
                    {
                        Debug.WriteLine("Magnetometer not available");
                    }
#else
                    Debug.WriteLine("Magnetometer not supported");
#endif
                  
                    break;
                case MotionSensorType.Compass:

                    if(compass != null)
                    {

                        compass.ReportInterval = delay;
                        compass.ReadingChanged += CompassReadingChanged;
                    }
                    else
                    {
                        Debug.WriteLine("Compass not available");
                    }

                    break;

            }
            sensorStatus[sensorType] = true;
        }

#if WINDOWS_PHONE_APP
        void MagnetometerReadingChanged(Magnetometer sender, MagnetometerReadingChangedEventArgs args)
        {
            if(SensorValueChanged != null)
                SensorValueChanged(this, new SensorValueChangedEventArgs { ValueType = MotionSensorValueType.Vector, SensorType = MotionSensorType.Magnetometer, Value = new MotionVector() { X = args.Reading.MagneticFieldX, Y = args.Reading.MagneticFieldY, Z = args.Reading.MagneticFieldZ } });
        }
#endif
        void CompassReadingChanged(Compass sender, CompassReadingChangedEventArgs args)
        {
            if (args.Reading.HeadingTrueNorth!=null &&SensorValueChanged!=null)
                SensorValueChanged(this, new SensorValueChangedEventArgs { ValueType = MotionSensorValueType.Single, SensorType = MotionSensorType.Compass, Value = new MotionValue() { Value = args.Reading.HeadingTrueNorth} });
        }
        void GyrometerReadingChanged(Gyrometer sender, GyrometerReadingChangedEventArgs args)
        {
            if (SensorValueChanged != null)
                SensorValueChanged(this, new SensorValueChangedEventArgs { ValueType= MotionSensorValueType.Vector, SensorType = MotionSensorType.Gyroscope, Value = new MotionVector() { X = args.Reading.AngularVelocityX, Y = args.Reading.AngularVelocityY, Z = args.Reading.AngularVelocityZ } });

        }

        void AccelerometerReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
        {
            if (SensorValueChanged != null)
                SensorValueChanged(this, new SensorValueChangedEventArgs { ValueType = MotionSensorValueType.Vector, SensorType = MotionSensorType.Accelerometer, Value = new MotionVector() { X = args.Reading.AccelerationX, Y = args.Reading.AccelerationY, Z = args.Reading.AccelerationZ } });
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
                    if(accelerometer!=null)
                    {
                       accelerometer.ReadingChanged -= AccelerometerReadingChanged;
                    }
                    else
                    {
                        Debug.WriteLine("Accelerometer not available");
                    }
                    break;
                case MotionSensorType.Gyroscope:
                    if (gyrometer != null)
                    {
                        gyrometer.ReadingChanged -= GyrometerReadingChanged;
                    }
                    else
                    {
                        Debug.WriteLine("Gyrometer not available");
                    }
                    break;
                case MotionSensorType.Magnetometer:
#if WINDOWS_PHONE_APP
                    if(magnetometer!=null)
                    {
                        magnetometer.ReadingChanged -= MagnetometerReadingChanged;
                    }
                    else
                    {
                        Debug.WriteLine("Magnetometer not available");
                    }
#else
                    Debug.WriteLine("Magnetometer not supported");
#endif
                    break;
                    case MotionSensorType.Compass:
                    if (compass != null)
                    {
                       compass.ReadingChanged -= CompassReadingChanged;
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
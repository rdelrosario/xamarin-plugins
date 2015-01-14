using System;

namespace DeviceMotion.Plugin.Abstractions
{
    /// <summary>
    /// Device motion Interface.
    /// </summary>
    public interface IDeviceMotion : IDisposable
    {
        /// <summary>
        /// Occurs when sensor value changed.
        /// </summary>
        event SensorValueChangedEventHandler SensorValueChanged;
        

        /// <summary>
        /// Start the specified sensorType and interval.
        /// </summary>
        /// <param name="sensorType">Sensor type.</param>
        /// <param name="interval">Interval.</param>
        void Start(MotionSensorType sensorType, MotionSensorDelay interval = MotionSensorDelay.Default);
        /// <summary>
        /// Stop the specified sensorType.
        /// </summary>
        /// <param name="sensorType">Sensor type.</param>
        void Stop(MotionSensorType sensorType);

        /// <summary>
        /// Determines whether this instance is active the specified sensorType.
        /// </summary>
        /// <returns><c>true</c> if this instance is active the specified sensorType; otherwise, <c>false</c>.</returns>
        /// <param name="sensorType">Sensor type.</param>
        bool IsActive(MotionSensorType sensorType);

	}
		
   /// <summary>
   /// Sensor changed event arguments.
   /// </summary>
	public class SensorValueChangedEventArgs  : EventArgs
    {

		/// <summary>
		/// Gets or sets the type of the sensor.
		/// </summary>
		/// <value>The type of the sensor.</value>
		public MotionSensorType SensorType { get; set; }
		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public MotionValue Value  { get; set; }
		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		/// <value>The type of the value.</value>
		public MotionSensorValueType ValueType { get; set; }

    }

    /// <summary>
    /// Sensor value changed event handler.
    /// </summary>
	public delegate void SensorValueChangedEventHandler(object sender, SensorValueChangedEventArgs e);
}

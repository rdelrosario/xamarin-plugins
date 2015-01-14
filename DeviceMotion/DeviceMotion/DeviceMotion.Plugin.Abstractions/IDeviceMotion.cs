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
    public class VectorValueSensorChangedEventArgs : BaseValueSensorChangedEventArgs
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public new MotionVector Value { get; set; }
    }

    public class SingleValueSensorChangedEventArgs : BaseValueSensorChangedEventArgs
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public new double?  Value { get; set; }
    }

    public abstract class BaseValueSensorChangedEventArgs : EventArgs
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
        public virtual object Value { get; set; }
    }

    [Obsolete("SensorChangedEventArgs is deprecated, please use BaseValueSensorChangedEventArgs instead.")]
    public class SensorChangedEventArgs : BaseValueSensorChangedEventArgs
    {

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public new MotionVector Value { get; set; }
    }

    /// <summary>
    /// Sensor value changed event handler.
    /// </summary>
    public delegate void SensorValueChangedEventHandler(object sender, BaseValueSensorChangedEventArgs e);
}

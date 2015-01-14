using System;

namespace DeviceMotion.Plugin.Abstractions
{
	/// <summary>
	/// Motion value.
	/// </summary>
	public class MotionValue
	{
		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public virtual double? Value { get; set; }

		/// <summary>
		/// Value to string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("Value = {0}",Value);
		}
	}
}


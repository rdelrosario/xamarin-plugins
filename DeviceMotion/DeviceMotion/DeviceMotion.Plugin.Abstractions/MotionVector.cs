using System;

namespace DeviceMotion.Plugin
{
	/// <summary>
	/// Motion vector.
	/// </summary>
	public class MotionVector
	{
		/// <summary>
		/// Gets or sets the x.
		/// </summary>
		/// <value>The x.</value>
		public double X { get; set; }
		/// <summary>
		/// Gets or sets the y.
		/// </summary>
		/// <value>The y.</value>
		public double Y { get; set; }
		/// <summary>
		/// Gets or sets the z.
		/// </summary>
		/// <value>The z.</value>
		public double Z { get; set; }

        /// <summary>
        /// Vector to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("X={0}, Y={0}, Z={0}",X,Y,Z);
        }
	}
}


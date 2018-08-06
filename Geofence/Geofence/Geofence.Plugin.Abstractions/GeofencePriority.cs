using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    /// <summary>
    /// Geofence Accuracy Precision Priority enum
    /// </summary>
    public enum GeofencePriority
    {
        /// <summary>
        /// Sets the location updates for balanced power accurancy basing location on Cells and WiFi spots.
        /// </summary>
        BalancedPower,
        /// <summary>
        /// Highest accuracy uses GPS and other sources to determine best location precision
        /// </summary>
        HighAccuracy,
        /// <summary>
        /// Acceptable accuracy 
        /// </summary>
        AcceptableAccuracy,
        /// <summary>
        /// Medium accuracy
        /// </summary>
        MediumAccuracy,
        /// <summary>
        /// Low accuracy
        /// </summary>
        LowAccuracy,
        /// <summary>
        /// Lowest Acurracy
        /// </summary>
        LowestAccuracy
    }
}

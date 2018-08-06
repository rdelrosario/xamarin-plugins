using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    /// <summary>
    /// Geofence update result Class
    /// </summary>
    public class GeofenceResult
    {
        /// <summary>
        /// Last time entered the geofence region
        /// </summary>
        public DateTime? LastEnterTime { get; set; }
        /// <summary>
        /// Last time exited the geofence region
        /// </summary>
        public DateTime? LastExitTime { get; set; }
        /// <summary>
        /// Result transition type
        /// </summary>
        public GeofenceTransition Transition { get; set; }
        /// <summary>
        /// Region identifier
        /// </summary>
        public string RegionId { get; set; }
        /// <summary>
        /// Duration span between last exited and entred time
        /// </summary>
        public TimeSpan? Duration { get { return LastExitTime - LastEnterTime; } }
        /// <summary>
        /// Time span between the last entry and current time.
        /// </summary>
        public TimeSpan? SinceLastEntry { get { return DateTime.Now - LastEnterTime; } }
        /// <summary>
        /// Result latitude
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Result longitude
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// Result accuracy
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// Transition message
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} geofence region: {1}", TransitionName, RegionId);
        }

        /// <summary>
        /// Get transition name
        /// </summary>
        public string TransitionName
        {
          get
          {
              switch (Transition)
              {

                  case GeofenceTransition.Entered:
                      return "Entered";

                  case GeofenceTransition.Exited:
                      return "Exited";

                  case GeofenceTransition.Stayed:
                      return "Stayed in";

                  default:
                      return "Unknown";
              }
          }
           
        }

        public GeofenceResult()
        {
        }

        public GeofenceResult(GeofenceResult result)
        {
            if (result != null)
            {
                this.LastEnterTime = result.LastEnterTime;
                this.LastExitTime = result.LastExitTime;
                this.Transition = result.Transition;
                this.RegionId = result.RegionId;
                this.Latitude = result.Latitude;
                this.Longitude = result.Longitude;
                this.Accuracy = result.Accuracy;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    public class GeofenceResult
    {
        public DateTime? LastEnterTime { get; set; }
        public DateTime? LastExitTime { get; set; }
        public GeofenceTransition Transition { get; set; }
        public GeofenceCircularRegion Region { get; set; }
        public TimeSpan? Duration { get { return LastExitTime - LastEnterTime; } }
        public TimeSpan? SinceLastEntry { get { return DateTime.UtcNow - LastEnterTime; } }

        public override string ToString()
        {
            return string.Format("{0} {1}: {2}",GetTransitionString(Transition),"geofence region",Region.Tag);
        }

        public static string GetTransitionString(GeofenceTransition transitionType)
        {
            switch (transitionType)
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
}

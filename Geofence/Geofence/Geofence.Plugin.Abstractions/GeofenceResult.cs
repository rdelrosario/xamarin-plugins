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
        public GeofenceCircularRegion Region { get; set; }
        public TimeSpan? Duration { get { return LastExitTime - LastEnterTime; } }
        public TimeSpan? SinceLastEntry { get { return DateTime.UtcNow - LastEnterTime; } }
    }
}

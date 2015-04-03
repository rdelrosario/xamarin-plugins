using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    public class GeofenceCircularRegion
    {
        public string Tag { get; set; }
        public double Latitude  { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
        public int MinimumDuration { get; set; }

        public override string ToString()
        {
            return Tag;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    public class GeoCircularRegion
    {
        public string Tag { get; set; }
        public double Latitude  { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    public class GeofenceLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Date { get; set; }
        public double Accuracy { get; set; }
    }
}

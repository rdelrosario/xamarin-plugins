using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    public enum GeofencePriority
    {
        BalancedPower,
        HighAccuracy,
        AcceptableAccuracy,
        MediumAccuracy,
        LowAccuracy,
        LowestAccuracy
    }
}

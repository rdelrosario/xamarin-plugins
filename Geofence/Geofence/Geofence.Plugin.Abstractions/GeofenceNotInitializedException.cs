using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    public class GeofenceNotInitializedException : Exception
    {
          public GeofenceNotInitializedException()
          {

          }

          public GeofenceNotInitializedException(string message) : base(message)
          {

          }
    }
}

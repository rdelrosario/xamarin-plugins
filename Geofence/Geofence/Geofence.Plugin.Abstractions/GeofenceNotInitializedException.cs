using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    /// <summary>
    /// GeofenceNotInitializedException Exception class
    /// Exception thown when plugin is not initialized
    /// </summary>
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

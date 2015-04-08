using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geofence.Plugin.Abstractions
{
    /// <summary>
    /// Geofence Region Class
    /// </summary>
    public class GeofenceCircularRegion
    {
        /// <summary>
        /// Region identifier
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Region center Latitude
        /// </summary>
        public double Latitude  { get; set; }
        /// <summary>
        /// Region center Longitude
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// Radius covered by the region in meters
        /// </summary>
        public double Radius { get; set; }
        /// <summary>
        /// Notify when enters region
        /// </summary>
        public bool NotifyOnEntry { get; set; }
        /// <summary>
        /// Notify when stays in region based on the milliseconds specified in StayedInThresholdDuration
        /// Note: Stayed in transition will not be fired if device has exit region before the StayedInThresholdDuration
        /// </summary>
        public bool NotifyOnStay { get; set; }
        /// <summary>
        /// Notify when exits region
        /// </summary>
        public bool NotifyOnExit { get; set; }
        /// <summary>
        /// Notitication message when enters region
        /// </summary>
        public string NotificationEntryMessage { get; set; }
        /// <summary>
        /// Notification message when exits region
        /// </summary>
        public string NotificationExitMessage { get; set; }
        /// <summary>
        /// Notification message when stays in region
        /// </summary>
        public string NotificationStayMessage { get; set; }
        /// <summary>
        /// Persist region so that is available after application closes
        /// </summary>
        public bool Persistent { get; set; }
        /// <summary>
        /// Enables showing local notifications 
        /// Messages could be configured using properties: NotificationEntryMessage, NotificationExitMessage, NotificationStayMessage
        /// </summary>
        public bool ShowNotification { get; set; }

        /// <summary>
        /// Sets minimum duration time in milliseconds before passing to stayed in transition after an entry 
        /// </summary>
        public int StayedInThresholdDuration;

   
        public override string ToString()
        {
            return Id;
        }
    }
}

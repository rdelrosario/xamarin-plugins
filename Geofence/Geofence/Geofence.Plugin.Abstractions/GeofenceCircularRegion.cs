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
        internal GeofenceCircularRegion()
        {

        }
        public GeofenceCircularRegion(string id, double centerLatitude, double centerLongitude, double radius, 
                                      bool notifyOnEntry = true, bool notifyOnExit = true, bool notifyOnStay = false, 
                                      bool showNotification = true, bool persistent = true, 
                                      bool showEntryNotification = true, bool showExitNotification = true, bool showStayNotification = true)
        {
            Id=id;
            Latitude = centerLatitude;
            Longitude = centerLongitude;
            Radius = radius;
            NotifyOnEntry = notifyOnEntry;
            NotifyOnExit = notifyOnExit;
            NotifyOnStay = notifyOnStay;
            ShowNotification = showNotification;
            Persistent = persistent;
            ShowEntryNotification = showEntryNotification;
            ShowExitNotification = showExitNotification;
            ShowStayNotification = showStayNotification;
        }
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
        /// Notify when stays in region based on the time span specified in StayedInThresholdDuration
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
        /// Enables showing local notifications. Defaults to showing all notifications, unless setting ShowEntry/Exit/StayNotification entries to false.
        /// Messages could be configured using properties: NotificationEntryMessage, NotificationExitMessage, NotificationStayMessage
        /// </summary>
        public bool ShowNotification { get; set; }
        /// <summary>
        /// Enables showing local notifications. ShowNotification must be true.
        /// Messages could be configured using properties: NotificationEntryMessage
        /// </summary>
        public bool ShowEntryNotification { get; set; }
        /// <summary>
        /// Enables showing local notifications. ShowNotification must be true.
        /// Messages could be configured using properties: NotificationExitMessage
        /// </summary>
        public bool ShowExitNotification { get; set; }
        /// <summary>
        /// Enables showing local notifications. ShowNotification must be true.
        /// Messages could be configured using properties: NotificationStayMessage
        /// </summary>
        public bool ShowStayNotification { get; set; }

        /// <summary>
        /// Sets minimum duration time span before passing to stayed in transition after an entry 
        /// </summary>
        public TimeSpan StayedInThresholdDuration;

        /// <summary>
        /// Sets the expiration duration milliseconds of geofence. This geofence will be removed automatically after this period of time.
        /// </summary>
        //public long ExpirationDuration;

        /// <summary>
        /// Sets minimum duration time span before passing to exit transition after an entry 
        /// </summary>
        public TimeSpan ExitThresholdDuration;

   
        public override string ToString()
        {
            return Id;
        }
    }
}

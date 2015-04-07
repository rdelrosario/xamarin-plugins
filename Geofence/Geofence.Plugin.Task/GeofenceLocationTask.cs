using Geofence.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace Geofence.Plugin.Task
{
    public sealed class GeofenceLocationTask : IBackgroundTask
    {
        static string TaskName = "GeofenceLocationTask";

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // Uncomment this to utilize async/await in the task
            //var deferral = taskInstance.GetDeferral();

            // Get the information of the geofence(s) that have been hit
            var reports = Windows.Devices.Geolocation.Geofencing.GeofenceMonitor.Current.ReadReports();
            var report = reports.FirstOrDefault(r => (r.Geofence.Id == "MicrosoftStudioE") && (r.NewState == Windows.Devices.Geolocation.Geofencing.GeofenceState.Entered));

            if (report == null) return;

            // Create a toast notification to show a geofence has been hit
            var toastXmlContent = Windows.UI.Notifications.ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            var txtNodes = toastXmlContent.GetElementsByTagName("text");
            txtNodes[0].AppendChild(toastXmlContent.CreateTextNode("Geofence triggered toast!"));
            txtNodes[1].AppendChild(toastXmlContent.CreateTextNode(report.Geofence.Id));

            var toast = new ToastNotification(toastXmlContent);
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            toastNotifier.Show(toast);
            CrossGeofence.GeofenceListener.OnRegionStateChanged(new GeofenceResult()
            {


            });
            // Uncomment this to utilize async/await in the task
            //deferral.Complete();
        }

        public async static void Register()
        {
            if (!IsTaskRegistered())
            {
                var result = await BackgroundExecutionManager.RequestAccessAsync();
                var builder = new BackgroundTaskBuilder();

                builder.Name = TaskName;
                builder.TaskEntryPoint = typeof(GeofenceTask).FullName;
                builder.SetTrigger(new LocationTrigger(LocationTriggerType.Geofence));

                // Uncomment this if your task requires an internet connection
                //var condition = new SystemCondition(SystemConditionType.InternetAvailable);
                //builder.AddCondition(condition);

                builder.Register();
            }
        }

        public static void Unregister()
        {
            var entry = BackgroundTaskRegistration.AllTasks.FirstOrDefault(kvp => kvp.Value.Name == TaskName);

            if (entry.Value != null)
                entry.Value.Unregister(true);
        }

        public static bool IsTaskRegistered()
        {
            var taskRegistered = false;
            var entry = BackgroundTaskRegistration.AllTasks.FirstOrDefault(kvp => kvp.Value.Name == TaskName);

            if (entry.Value != null)
                taskRegistered = true;

            return taskRegistered;
        }
    }
}

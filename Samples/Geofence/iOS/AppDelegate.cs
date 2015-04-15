using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Geofence.Plugin;
using CrossGeofenceSample.Helpers;
using Acr.UserDialogs;
using System.Diagnostics;

namespace CrossGeofenceSample.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{

			global::Xamarin.Forms.Forms.Init ();

			UserDialogs.Init ();

			Xamarin.FormsMaps.Init();
		
			CrossGeofence.Initialize<CrossGeofenceListener> ();

			LoadApplication (new App ());


			return base.FinishedLaunching (app, options);
		}
	}
}


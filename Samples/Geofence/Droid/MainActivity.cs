using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Acr.UserDialogs;

namespace CrossGeofenceSample.Droid
{
	[Activity (Label = "CrossGeofenceSample", LaunchMode=LaunchMode.SingleTask, ScreenOrientation = ScreenOrientation.Portrait, Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);

			UserDialogs.Init (() => this);

			Xamarin.FormsMaps.Init(this,bundle);

			LoadApplication (new App ());

		
		}
	}
}


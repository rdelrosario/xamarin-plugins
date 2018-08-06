using System;

using Xamarin.Forms;
using Geofence.Plugin;
using Geofence.Plugin.Abstractions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using CrossGeofenceSample.Helpers;


namespace CrossGeofenceSample
{
	public class App : Application
	{
		public static ObservableCollection<Event> Events=Settings.GetEvents();

		public enum AppState
		{
			Foreground,
			Background
		}
		public App ()
		{
			// The root page of your application
			MainPage = new HomePage();
		}

		protected override void OnStart ()
		{
			//Application.Current.Properties ["AppState"] = AppState.Foreground;
			//SavePropertiesAsync ();
		

		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
			//Application.Current.Properties ["AppState"] = AppState.Background;
			//SavePropertiesAsync ();
		}

		protected override void OnResume ()
		{

			// Handle when your app resumes
			//Application.Current.Properties ["AppState"] = AppState.Foreground;
			//SavePropertiesAsync ();
		}
	}
}


using System;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using MapKit;
using Xamarin.Forms.Maps;
using System.Diagnostics;
using Xamarin.Forms.Maps.iOS;

[assembly: ExportRenderer(typeof(Map), typeof(CrossGeofenceSample.CustomMapRenderer))]
namespace CrossGeofenceSample
{
	public class CustomMapRenderer  : MapRenderer
	{
		protected override void OnElementChanged (ElementChangedEventArgs<View> e)
		{
			base.OnElementChanged (e);
			Debug.WriteLine ("Map");

			MKMapView mkMapView = (MKMapView)Control;
			mkMapView.ShowsUserLocation = true;
			mkMapView.UserTrackingMode = MKUserTrackingMode.FollowWithHeading;
		}


	}
}


using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Diagnostics;
using Xamarin.Forms.Maps;
using Geofence.Plugin.Abstractions;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Geofence.Plugin;

namespace CrossGeofenceSample
{
	public partial class MapPage : ContentPage
	{
		bool isInitialized=false;
		public static readonly BindableProperty LatitudeProperty=BindableProperty.Create<MapPage, double>( p => p.Latitude, 0.0);
		public static readonly BindableProperty LongitudeProperty=BindableProperty.Create<MapPage,double>( p => p.Longitude, 0.0);


		public double Latitude
		{
			get 
			{ 

				return (double)GetValue(LatitudeProperty); 
			}
			set 
			{
				SetValue(LatitudeProperty, value); 


			}
		}

		public double Longitude
		{
			get 
			{ 
				return (double)GetValue(LongitudeProperty); 
			}
			set 
			{
				SetValue(LongitudeProperty, value); 
			}
		}

	
		public MapPage ()
		{
			InitializeComponent ();
			MessagingCenter.Subscribe<MapViewModel,Place> (this, "AddPin", (s, a) => {
				AddPin(a);
			});
			MessagingCenter.Subscribe<MapViewModel> (this, "ClearPins", (s) => {
				if(isInitialized){
					mapView.Pins.Clear();
				}
			
			});


		}
		protected override void OnAppearing ()
		{
			base.OnAppearing ();

			if (!isInitialized) 
			{
				mapView.IsShowingUser = true;
				double lat = 18.4802878;
				double lon=-69.9469203;

				if (CrossGeofence.Current.LastKnownLocation != null) 
				{
					lat = CrossGeofence.Current.LastKnownLocation.Latitude;
					lon =CrossGeofence.Current.LastKnownLocation.Longitude;
				}
				mapView.MoveToRegion (
					MapSpan.FromCenterAndRadius (
						new Xamarin.Forms.Maps.Position (lat, lon), Distance.FromMiles (1)).WithZoom (20));
				mapView.HasZoomEnabled = isInitialized;
				mapView.HasScrollEnabled = isInitialized;

				mapView.PropertyChanged+= (sender, e) => {
					if(e.PropertyName=="VisibleRegion")
					{
						Latitude=mapView.VisibleRegion.Center.Latitude;
						Longitude=mapView.VisibleRegion.Center.Longitude;
						mapView.VisibleRegion.WithZoom (20);

						if(!isInitialized)
						{
							isInitialized=true;

							if (CrossGeofence.Current.LastKnownLocation != null) 
							{
								double lat1 = CrossGeofence.Current.LastKnownLocation.Latitude;
								double lon1 =CrossGeofence.Current.LastKnownLocation.Longitude;
								mapView.MoveToRegion (
									MapSpan.FromCenterAndRadius (
										new Xamarin.Forms.Maps.Position (lat1, lon1), Distance.FromMiles (1)).WithZoom (20));
								
							}

							LoadData();

						}
					}
					Debug.WriteLine(e.PropertyName);
				};
			}


		}
		async Task LoadData()
		{
			viewModel.IsBusy = true;
			await Task.Delay (2000);

			foreach(var p in viewModel.Data)
			{
				AddPin(p);
			}
			viewModel.IsBusy = false;
			mapView.HasZoomEnabled = isInitialized;
			mapView.HasScrollEnabled = isInitialized;
		}
		void AddPin(Place a)
		{
			var position = new Xamarin.Forms.Maps.Position(a.Latitude,a.Longitude); 
			var pin = new Pin {
				Type = PinType.Place,
				Position = position,
				Label =a.Name,
				Address=string.Format("Radius: {0}",a.Radius)

			};
			mapView.Pins.Add(pin);
			Debug.WriteLine("TTTT");

		}
	}
}


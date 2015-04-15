using System;
using PropertyChanged;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;
using Acr.UserDialogs;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Geofence.Plugin;
using Geofence.Plugin.Abstractions;

namespace CrossGeofenceSample
{
	[ImplementPropertyChanged]
	public class MapViewModel
	{
		public MapViewModel ()
		{
			Data = new ObservableCollection<Place> ();


			//Restored regions
			foreach (var region in CrossGeofence.Current.Regions.Values) 
			{
				var place = new Place () {
					Name = region.Id,
					Latitude = region.Latitude,
					Longitude = region.Longitude,
					Radius = region.Radius,
				};
				Data.Add (place);

			}
				
			IsBusy = true;
		}

		public ObservableCollection<Place> Data { get; set;}

		public bool IsBusy { get; set;}

		public ICommand EditCommand
		{
			get
			{

				return new Command ( () => {

					IsEditing = true;
					Debug.WriteLine(Latitude);
					Debug.WriteLine(Longitude);

				});
			}
		}

		public ICommand CancelCommand
		{
			get
			{

				return new Command (() => {

					IsEditing = false;

				});
			}
		}
		public ICommand AddCommand
		{
			get
			{

				return new Command (async () => {

					await AddGeofence();

				});
			}
		}
		public ICommand ClearCommand
		{
			get
			{

				return new Command ( async() => {
					bool result=await UserDialogs.Instance.ConfirmAsync("Are you sure you would like to clear all regions monitoring and events data?","Clear All Data");
					if(result)
					{
						if (CrossGeofence.Current.IsMonitoring)
						{
							CrossGeofence.Current.StopMonitoringAllRegions();

							MessagingCenter.Send<MapViewModel>(this,"ClearPins");
						} 

					}
				
				});
			}
		}
		public bool IsEditing { set; get;}

		public bool IsNotEditing { get { return !IsEditing&&IsNotBusy; }}

		public bool IsNotBusy { get { return !IsBusy; }}

		public double Latitude{ set; get;}

		public double Longitude { set; get;}

	

		private async Task AddGeofence()
		{
			PromptResult result=await UserDialogs.Instance.PromptAsync ("Enter geofence region name", "Add Geofence");

			if (result.Ok) {
				
				try {
					PromptResult result1 = await UserDialogs.Instance.PromptAsync ("Enter radius (in meters)", "Geofence Radius", "Ok", "Cancel", "", InputType.Number);
					double radius = 50;
					if (!string.IsNullOrEmpty (result1.Text)) {
						radius = double.Parse (result1.Text);
					}


					UserDialogs.Instance.ShowLoading ();

					var place=new Place () {
						Name = result.Text,
						Latitude = Latitude,
						Longitude = Longitude,
						Radius = radius
					};


					Data.Add (place);
					CrossGeofence.Current.StartMonitoring(new GeofenceCircularRegion (place.Name,place.Latitude,place.Longitude,place.Radius) {

						NotifyOnStay=true,
						StayedInThresholdDuration=TimeSpan.FromMinutes(5)

					});


					MessagingCenter.Send<MapViewModel,Place>(this,"AddPin",place);

					UserDialogs.Instance.HideLoading ();

					await UserDialogs.Instance.AlertAsync(string.Format("{0} geofence region added!",place.Name));


				} catch (Exception ex) {
					Debug.WriteLine (ex.ToString ());
				}finally{
					IsEditing = false;
				}



			} else {
				IsEditing = false;
			}
		}
	
	}
}


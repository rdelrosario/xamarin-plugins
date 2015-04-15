using System;
using PropertyChanged;
using System.Collections.ObjectModel;
using CrossGeofenceSample.Helpers;
using System.Windows.Input;
using Xamarin.Forms;

namespace CrossGeofenceSample
{
	[ImplementPropertyChanged]
	public class EventsViewModel
	{
		public EventsViewModel ()
		{
			
		}

		public ObservableCollection<Event> Data { get {return App.Events;}}
	
	}
}


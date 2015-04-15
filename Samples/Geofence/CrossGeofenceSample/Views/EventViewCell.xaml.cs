using System;
using System.Collections.Generic;
using Xamarin.Forms;
using CrossGeofenceSample.Helpers;

namespace CrossGeofenceSample
{
	public partial class EventViewCell : ViewCell
	{
		public EventViewCell ()
		{
			InitializeComponent ();
		}

		void OnDelete(object sender,EventArgs args)
		{
			var e = (Event)this.BindingContext;

			App.Events.Remove (e);
			Settings.RemoveEvent (e);
		}
	}
}


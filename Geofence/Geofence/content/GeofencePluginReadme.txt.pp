Should initialize plugin before use in order to handle geofence on iOS, so that can be delegated to a class that implements IGeofenceListener
You must include CrossGeofence initialization on your AppDelegate FinishedLaunching method. 
     
       Example:

    public override bool FinishedLaunching (UIApplication app, NSDictionary options)
	{
		    //Initialization here...
			
            CrossPushGeofence.Initialize<CrossGeofenceListener> ();

		    return base.FinishedLaunching (app, options);
	}
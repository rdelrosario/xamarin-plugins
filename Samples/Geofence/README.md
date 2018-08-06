xamarin-plugins 
===============

## Geofence Plugin for Xamarin Sample

This is a sample of the use of the Geofence plugin in a PCL Project.

### Configuration:

#### Android

To use the Google Maps API on Android you must generate an **API key** and add it to your Android project. See the Xamarin doc on [obtaining a Google Maps API key](http://developer.xamarin.com/guides/android/platform_features/maps_and_location/maps/obtaining_a_google_maps_api_key/). After following those instructions, paste the **API key** in the `Properties/AndroidManifest.xml` file (view source and find/update the following element):

    <meta-data android:name="com.google.android.maps.v2.API_KEY" android:value="ApiKeyValueGoesHere" />

You need to follow these instructions in order for the map data to display in CrossGeofenceSample on Android.

Without a valid API key the maps control will display as a grey box on Android.

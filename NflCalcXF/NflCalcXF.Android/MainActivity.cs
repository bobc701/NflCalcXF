using System;

using Android.App;
using Android.Content.PM;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
using Android.OS;
using Android.Util;

namespace NflCalcXF.Droid
{
    // I added ScreenOrientation = ScreenOrientation.Portrait here, per Stack Overflow, 
    // to lock into portrait...
    [Activity(Label = "What If?", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            try {
               Log.Info("NflCalcXF", "In MainActivity.OnCreate");
               TabLayoutResource = Resource.Layout.Tabbar;
               ToolbarResource = Resource.Layout.Toolbar;

               base.OnCreate(bundle);

               global::Xamarin.Forms.Forms.Init(this, bundle);
               LoadApplication(new App()); 
            }
            catch (Exception ex) {
               Log.Error("NflCalcXF", "Error in MainActivity.OnCreate:" + ex.Message);
            }
        }
    }
}


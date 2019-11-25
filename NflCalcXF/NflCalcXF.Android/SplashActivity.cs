using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Android.App;
using Android.Content;
//using Android.Content.PM;
using Android.Support.V7.App;
using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
using Android.Util;

namespace NflCalcXF.Droid {

   [Activity(Theme = "@style/MainTheme.Splash", MainLauncher = true, NoHistory = true)]
   public class SplashActivity : AppCompatActivity {
      static readonly string TAG = "X:" + typeof(SplashActivity).Name;

      public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState) {
         //base.OnCreate(savedInstanceState, persistentState);
         Log.Info("NflCalcXF", "SplashActivity.OnCreate");
      }

      // Launches the startup task
      protected override void OnResume() {
         try {
            base.OnResume();
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
         }
         catch (Exception ex) {
            Log.Error("NflCalcXF", "Error in SplashActivity.OnResume:" + ex.Message);
         }

      }


      //// Launches the startup task
      //protected override void OnResume() {
      //   base.OnResume();
      //   Task startupWork = new Task(() => { SimulateStartup(); });
      //   startupWork.Start();
      //}

      //// Simulates background work that happens behind the splash screen
      //async void SimulateStartup() {
      //   try {
      //      Log.Info("NflCalcXF", "Performing some startup work that takes a bit of time.");
      //      await Task.Delay(2000); // Simulate a bit of startup work.
      //      Log.Info("NflCalcXF", "Startup work is finished - starting MainActivity.");
      //      StartActivity(new Intent(Application.Context, typeof(MainActivity)));
      //   }
      //   catch (Exception ex) {
      //      Log.Error("NflCalcXF", "Error in SplashActivity.SimulateStartup:" + ex.Message);
      //   }
      //}

   }

}
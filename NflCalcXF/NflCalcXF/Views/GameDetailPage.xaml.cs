#define PAID 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using NflCalcXF.ViewModels;
using NflCalc;

using static NflCalcXF.Services.Repository;
using NflCalcXF;



namespace NflCalcXF.Views {

   public partial class GameDetailPage : ContentPage {
#if LITE
      string msg =
         "The game override feature is only available on the " +
         "paid version of Pro Football What-If. The paid version " +
         "is available in the Google Play Store.";
#endif
      GameDetailViewModel gameDet;
      public GameDetailPage(CGame g) {
         // ------------------------------------------
         InitializeComponent();
         this.BindingContext = gameDet = new GameDetailViewModel(g);
         if (g.Override == EResult.HomeWin) HomeOverride.IsToggled = true;
         else if (g.Override == EResult.VisWin) VisOverride.IsToggled = true;
      }


      async private void OnVisTogged(object sender, ToggledEventArgs e) {
         // ---------------------------------------------------------------
#if LITE
         Switch s = (Switch)sender;
         if (s.IsToggled) {
            s.IsToggled = false;
            await DisplayAlert("", msg, "OK");
         }
#else
         if (e.Value && HomeOverride.IsToggled) {
            HomeOverride.IsToggled = false;
         }
         gameDet.g.Override = e.Value ? EResult.VisWin : EResult.NotPlayed;
#endif
      }

      async private void OnHomeToggled(object sender, ToggledEventArgs e) {
         // ---------------------------------------------------------------
#if LITE
         Switch s = (Switch)sender;
         if (s.IsToggled) {
            s.IsToggled = false;
            await DisplayAlert("", msg, "OK");
         }
#else
         if (e.Value && VisOverride.IsToggled) {
            VisOverride.IsToggled = false;
         }
         gameDet.g.Override = e.Value ? EResult.HomeWin : EResult.NotPlayed;
#endif
      }


      async private void DoCalc2_OnClick(object sender, EventArgs e) {
         // ----------------------------------------------------------
         //int res = await FetchData(); 
         //int res = await Task.Run(() => { return season.FetchData(); }); // No mgt of busy indicator here
         int res = await season.FetchData();

         if (res != 0) await DisplayAlert("Retrieving Data", GetMessage(res), "OK");
         if (res == -1) return;

         season.SeasonComplete += t => pb_Simulation.Progress = 0.001 * t;

         pb_Simulation.Progress = 0.0;
         pb_Simulation.HeightRequest = 10.0;
         pb_Simulation.IsVisible = true;
         await Task.Run(() => { season.Simulate(); });

         //season.Simulate();
         var s = season.resultsString.ToString();
         const string sPreface =
            "Pro Football What-if just simulated the\r\n" +
            "remainder of the season 1000 times!\r\n" +
            "Here are the aggregated results... \r\n" +
            "\r\n" +
            "w-l-t: Current actual record\r\n" +
            "POff: % chance of making playoffs\r\n" +
            "Div: % chance of winning division\r\n" +
            "Conf: % chance of winning conference\r\n" +
            "SB: % chance of winning Super Bowl";

         const string sFooter = "";
         await Navigation.PushAsync(new ResultsPage(s, "1000 Years", sPreface, sFooter));
         pb_Simulation.IsVisible = false; //-bc 1807.01

      }

      private string GetMessage(int res) {
      // --------------------------------------------------------------

         switch (res) {
            case -1: return $"Unable to read schedule and results from {SiteUsed}";
            case 0: return "";
            case 1:
               return
                  $"Unable to read data from {SiteUsed}.\r\n" +
                  "Will continue with possibly outdated results";
            default: return "Unexpected result";
         }
      }


      //async private Task<int> FetchData() {
      //   // ---------------------------------------------------------
      //   // This just calls season.FetchData, and manages the ActivityIndicator.
      //   // ---------------------------------------------------------
      //   //busyIndicator.IsVisible = true;
      //   //busyIndicator.IsRunning = true;

      //   int res = await Task.Run(() => { return season.FetchData(); });

      //   //busyIndicator.IsVisible = false;
      //   //busyIndicator.IsRunning = false;

      //   return res;

      //}


   }

}
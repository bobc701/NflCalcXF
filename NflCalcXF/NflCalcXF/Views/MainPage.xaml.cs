using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using NflCalc;
using static NflCalcXF.Services.Repository;
using NflCalcXF.Views;
using NflCalcXF.ViewModels;

//[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace NflCalcXF
{
    //[Android.Runtime.Preserve(AllMembers = true)]
    public partial class MainPage : ContentPage {
      
      public MainPage() {
         InitializeComponent();
         GetSeason();
         //this.Title = "";
         var dtm = DateTime.Now;
         this.Title = $"{(dtm.Month >= 5 ? dtm.Year : dtm.Year - 1).ToString()} Season";
         
      }

      async void DoCalc_OnClick(object sender, EventArgs e) {
      // -----------------------------------------------------------------------
         //int res = await FetchData();
         busyIndicator.IsVisible = true;
         busyIndicator.IsRunning = true;

         int res = await Task.Run(() => { return season.FetchData(); });

         busyIndicator.IsVisible = false;
         busyIndicator.IsRunning = false;

         if (res != 0) await DisplayAlert("Retrieving Data", GetMessage(res), "OK"); 
         if (res == -1) return;

         season.PlaySeason(debug: true);
         //Debug.WriteLine(season.resultsString.ToString());
         var s = season.resultsString.ToString();
         var sPreface =
            "Results of a single simulated season. \r\n" +
            "This is only to help illustrate how Pro Football What-if operates. " +
            "For more meaningfull results, run '1000-year Simulation' \r\n\r\n" +
            "Tip: Scroll to the bottom to see the playoffs and the Super Bowl.";
         var sFooter = "";
         await Navigation.PushAsync(new ResultsPage(s, "Single Year", sPreface, sFooter));

      }


      async private void btnWeeklySched_OnClick(object sender, EventArgs e) {
      // ------------------------------------------------------------------------
         //int res = await FetchData();
         busyIndicator.IsVisible = true;
         busyIndicator.IsRunning = true;

         //int res = season.FetchData(); 
         int res = await Task.Run(() => { return season.FetchData(); });

         busyIndicator.IsVisible = false;
         busyIndicator.IsRunning = false;

         if (res != 0) await DisplayAlert("Retrieving Data", GetMessage(res), "OK"); 
         if (res == -1) return;

         Button b = sender as Button;
         int weekNo = int.Parse(b.Text);
         await Navigation.PushAsync(new WeeklySchedulePage(weekNo));

      }


      async private void DoCalc2_OnClick(object sender, EventArgs e) {
      // ----------------------------------------------------------
         //int res = await FetchData(); // <-- Suggestion: put content of this in-line, avoid 1 await

         busyIndicator.IsVisible = true;
         busyIndicator.IsRunning = true;

         //int res = season.FetchData(); 
         int res = await Task.Run(() => { return season.FetchData(); });

         busyIndicator.IsVisible = false;
         busyIndicator.IsRunning = false;

         if (res != 0) await DisplayAlert("Retrieving Data", GetMessage(res), "OK");
         if (res == -1) return;

         season.SeasonComplete += t => pb_Simulation.Progress = 0.001 * t;

         pb_Simulation.Progress = 0.0;
         pb_Simulation.HeightRequest = 10.0;
         pb_Simulation.IsVisible = true;
         await Task.Run(() => { season.Simulate(); }); 
         // Suggestion: Don't run asyncronous, the Simulate can just fire event every 50 loops.

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
            case -1: return $"Unable to read schedule and results from {NflCalcXF.Services.Repository.SiteUsed}";
            case 0: return "";
            case 1:
               return
                  $"Unable to read data from {NflCalcXF.Services.Repository.SiteUsed}\r\n" +
                  "Will continue with possibly outdated results";
            default: return "Unexpected result";
         }
      }


      //async private Task<int> FetchData() {
      //   // ---------------------------------------------------------
      //   // This just calls season.FetchData, and manages the ActivityIndicator.
      //   // ---------------------------------------------------------
      //   busyIndicator.IsVisible = true;
      //   busyIndicator.IsRunning = true;

      //   //int res = season.FetchData(); 
      //   int res = await Task.Run(() => { return season.FetchData(); });
      
      //   busyIndicator.IsVisible = false;
      //   busyIndicator.IsRunning = false;

      //   return res;

      //}

      async private void Help_OnClick(object sender, EventArgs e) {

         await Navigation.PushAsync(new HelpPage());
      

      }

      async private void About_OnClick(object sender, EventArgs e) {

         await Navigation.PushAsync(new AboutPage());

      }

   }

}

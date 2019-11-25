using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using NflCalc;
using NflCalcXF.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NflCalcXF.Views
{
    
    public partial class WeeklySchedulePage : ContentPage
    {
       public WeeklyScheduleViewModel Sched { get; set; }

       public WeeklySchedulePage(int weekNo)
       {
            InitializeComponent();
            this.Title = "Week " + weekNo.ToString() + " Schedule";
            Sched = new WeeklyScheduleViewModel(weekNo);
  
            this.BindingContext = Sched;

            //Tried this --1911.01
            //Appearing += delegate (object sender, EventArgs e) {
            //   MyListView.SelectedItem = null;
            //}; 

         }

       async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
       {
          if (e.Item == null)
             return;

          CGame g1 = e.Item as CGame;
          if (g1.OrigResult != EResult.NotPlayed) {
            MyListView.SelectedItem = null; //Suppress selection --1911.01
            await DisplayAlert("",
                "This game has already been played,\r\nso you can't force the outcome.", "Got it!");
             return;
          }
            
          await Navigation.PushAsync(new GameDetailPage(g1));
          MyListView.SelectedItem = null; //Suppress selection --1911.01


         // await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

      }

    }
}

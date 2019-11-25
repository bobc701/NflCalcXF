using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using NflCalc;
using static NflCalcXF.Services.Repository;

namespace NflCalcXF.ViewModels
{
   public class WeeklyScheduleViewModel {

      public int WeekNo { get; set; }
      public ObservableCollection<CGame> WeeklySched { get; set; } = new ObservableCollection<CGame>();


      public WeeklyScheduleViewModel(int n) {
         // ------------------------------------------------------------
         WeekNo = n;
         CGame[] sched = season.regSeason.Where(g => g.WeekNo == n).ToArray();
         foreach (CGame g in sched) {
            WeeklySched.Add(g);
         }

      }

   }
}

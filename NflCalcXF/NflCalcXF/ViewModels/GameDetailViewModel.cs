using System;
using System.Collections.Generic;
using System.Text;
using NflCalc;
using System.Collections.ObjectModel;

namespace NflCalcXF.ViewModels
{
   class GameDetailViewModel {

      public CGame g { get; set; }

      public GameDetailViewModel(CGame g1) {

         g = g1;

      }

      public string ResultHeader {

         get {
            return
               string.Format("Week {0}: {1} at {2}",
                  g.WeekNo, g.TeamV.teamTag, g.TeamH.teamTag);

         }

      }

   }
}

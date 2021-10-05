using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NflCalc {




   public class CSpreadTable1 {
   // ---------------------------------------------------
   // This version of CSpreadTable uses a List object internally.
   // NOT every spread value needs to be in the table.
   // The table must go higher than any possible spread.
   // ---------------------------------------------------

      struct CEntry {
      // ------------------------------------
         public double spread;
         public double prob;
      }   

      private List<CEntry> table = new List<CEntry>();


      public double GetHomeProb(double spread) {
      // ----------------------------------------------
         CEntry e = table.Where(e1 => Math.Abs(spread) <= e1.spread).First();
         return spread >= 0.0 ? 0.01*e.prob : 1.0 - 0.01*e.prob;
      }


      public void ReadSpreadTable(StringReader sr) {
      // ---------------------------------------------
         string rec;
         while ((rec = sr.ReadLine()) != null) {
            if (rec.Trim() == "") {
               sr.Close();
               return;
            }
            string[] arec = rec.Trim().Split(new char[] {' ','%'}, StringSplitOptions.RemoveEmptyEntries);
            CEntry e = new CEntry {spread = double.Parse(arec[0]), prob = double.Parse(arec[1])};
            table.Add(e);
         }
      }

   }


   public class CSpreadTable2 {
   // ---------------------------------------------------
   // This version of CSpreadTable uses a Dictionary object internally.
   // Every 1/2 pount spread value up to MaxSpread must be in the table.
   // --------------------------------------------------

      private Dictionary<double, double> table = new Dictionary<double, double>();
      private double MaxSpread;


      public double GetHomeProb(double spread) {
      // ----------------------------------------------
         double p;
         if (Math.Abs(spread) >= MaxSpread) p = 1.0;
         else p = 0.01 * table[Math.Abs(spread)];
         return spread >= 0 ? p : 1.0 - p;
      }


      public void ReadSpreadTable(StreamReader f) {
      // ---------------------------------------------
         string rec; double spread = 0.0, prob;
         while ((rec = f.ReadLine()) != null) {
            if (rec.Trim() == "") {
               f.Close();
               this.MaxSpread = spread;
               return;
            }
            string[] arec = rec.Trim().Split(new char[] {' ','%'}, StringSplitOptions.RemoveEmptyEntries);
            spread = Double.Parse(arec[0]);
            prob = Double.Parse(arec[1]);
            table.Add(spread, prob);
         }
         
      }


   }

}

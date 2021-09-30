using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NflCalc {

   public class CTeam {
   // -----------------------------------------------
      public string teamTag { get; set; }
      public short teamIx;
      public string divName { get; set; }    //Eg: NFC-E - So you don't need conf to identify div.
      public string confName { get; set; }   //Eg: NFC
      public string cityName;
      public string nickName;
      public double powerRating;
      public RecordSet realRec, simRec;
      public int Seed;

      public CAccumulators tots = new CAccumulators();
      public List<CGame> teamSchedule = new List<CGame>();
      public string Metric;
      
      //public void BumpSim(char res) {
      //   switch (res) {
      //      case 'w': sim.reg.w++; break;
      //      case 'l': sim.div.l++; break;
      //      case 't': sim.conf.t++; break;
      //   }
      //}

      //public void BumpReal(char res) {
      //   switch (res) {
      //      case 'w': real.reg.w++; break;
      //      case 'l': real.div.l++; break;
      //      case 't': real.conf.t++; break;
      //   }
      //}

      //public void Bump(char res, EMode mode) {

      //   switch (mode) {
      //      case EMode.Sim:
      //         switch (res) {
      //            case 'w': simRec.reg.w++; break;
      //            case 'l': simRec.div.l++; break;
      //            case 't': simRec.conf.t++; break;
      //         }
      //         break;
      //      case EMode.Real:
      //         switch (res) {
      //            case 'w': realRec.reg.w++; break;
      //            case 'l': realRec.div.l++; break;
      //            case 't': realRec.conf.t++; break;
      //         }
      //         break;
      //   }

      //}


      public CTeam(short ix, string tag, string conf1, string div1, double rtg) { 
         teamTag = tag;
         teamIx = ix;
         confName = conf1;
         divName = div1;
         powerRating = rtg;
   }


      public string FullName {
         get { return cityName + ' ' + nickName; }
      }


      /// <summary>
      /// This maps a team ID to an index (Like 'GBY' --> 12)
      /// </summary>
      public static Dictionary<string, short> teamIndexes;


      public List<CTeam> Opponents {
      // ------------------------------------------------------
         get {
            CTeam opp;
            List<CTeam> opps = new List<CTeam>();
            foreach (CGame g in this.teamSchedule) {
               opp = g.Opp(this);
               if (!opps.Contains(opp)) opps.Add(opp);
            }
            return opps;
         }

      }

}

   public struct RecordSet {
   // -----------------------------------------------
   // This can be either EMode.Real or .Sim
      public Record league;
      public Record div;
      public Record conf;
      public Record h2h;
      public void Reset() {
         league.Reset(); div.Reset(); conf.Reset(); h2h.Reset();
      }

   }
   

   public struct Record {
   // ----------------------------------------------
      public int w, l, t;
      public void Reset() { w = l = t = 0; }
      public double pct {
         get {
            if (w + l + t != 0) return (w + 0.5 * t) / (w + l + t);
            else return -1.0;
         }
      }
      public string spct {
         get {
            if (pct < 0.0) return "XXX";
            return pct == 1.0 ? "999" : string.Format("{0:000}", Math.Truncate(1000 * pct));
         }
      }

      public string wltString {
         get {return w.ToString() + "-" + l.ToString() + "-" + t.ToString(); }
      }

      public string wltStringFix {
         get {return string.Format("{0,2} {1,2} {2,2}", w, l, t); }
      }


      public static Record operator +(Record r1, Record r2) {
      // -------------------------------------------------------
         Record result = default(Record);
         result.w = r1.w + r2.w;
         result.l = r1.l + r2.l;
         result.t = r1.t + r2.t;
         return result;
      }


   }




   public struct CAccumulators {
      // ---------------------------------------------------------------

      public int Playoffs, Div, Conf, SB;

      public void Zero() { Playoffs=0; Div=0; Conf=0; SB=0; }

   }





}

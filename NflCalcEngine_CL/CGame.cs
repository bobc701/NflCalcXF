using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static System.Math;

namespace NflCalc {

   public enum EResult { NotPlayed, HomeWin, VisWin, Tie }
   public enum ESite { Home, Nuetral }
   public enum EMode { Real, Sim }


   public class CGame : INotifyPropertyChanged {

      public event PropertyChangedEventHandler PropertyChanged;

      public int WeekNo { get; set; }
      public int GameNumber { get; set; }
      public int TeamIdV, TeamIdH;
      public CTeam TeamV { get; set; } 
      public CTeam TeamH { get; set; }
      public int ScoreV, ScoreH;
      public ESite Site;
      public EResult Result;
      public EResult OrigResult;
      public double? Spread;
      public double HomeProb;
      public CTeam Winner, Loser;
      public bool WasTie;

      private EResult _Override;
      public EResult Override { 
         get { return _Override; } 
         set { 
            _Override = value; 
            OnPropertyChanged(); 
            OnPropertyChanged("ResultString");
            OnPropertyChanged("ResultIcon"); //-bc 1807.01 added this so badge updated on return to schedule page
         } 
      } 


      protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
      // -----------------------------------------------------------------------------
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }


      public /*property*/ string ResultString {
      // ------------------------------------------------
         get {
            if (OrigResult == EResult.NotPlayed) { //-bc 1807.01 was just Result
               //Debug.WriteLine("OrigResult = " + OrigResult.ToString());
               switch (Override) {
                  case EResult.HomeWin: return " Forced: " + TeamH.teamTag;
                  case EResult.VisWin: return " Forced: " + TeamV.teamTag;
                  case EResult.NotPlayed:
                     // Game is not played and not overriden, will be simulated...
                     var fmtSpread = this.FmtSpread; 
                     if (fmtSpread == "even" || fmtSpread == "unk")
                        return " Favorite: " + fmtSpread;
                     else if (HomeProb >= 0.5)
                        //return " Simulate: " + TeamH.teamTag + ", " + (100.0*HomeProb).ToString("#0.0") + "%";
                        return " Favorite: " + TeamH.teamTag + " " + fmtSpread;
                     else
                        //return " Simulate: " + TeamV.teamTag + ", " + (100.0 - 100.0 * HomeProb).ToString("#0.0") + "%";
                        return " Favorite: " + TeamV.teamTag + " " + fmtSpread;
               }

            }
            else
               //Debug.WriteLine("OrigResult = " + OrigResult.ToString());
               switch (OrigResult) { //-bc1807.01
                  case EResult.HomeWin: return " Winner: " + TeamH.teamTag;
                  case EResult.VisWin: return " Winner: " + TeamV.teamTag;
                  case EResult.Tie: return "Tie";
               }

            return "Error";

         }

      }

      private string FmtSpread {
      // ----------------------------------------------------------------
         get {
            const char oneHalf = (char)189;
            string res;
            if (!Spread.HasValue) return "unk";
            double n = Abs(0.5 * Round(2.0 * Spread.Value, 0));
            double f = Truncate(n);
            if (n == f) res = n.ToString();
            else res = f.ToString() + oneHalf.ToString();
            if (res == "0") res = "even";
            if (res[0] == '0') res = oneHalf.ToString();
            if (res != "even") res = "-" + res;
            return res;
         }

      }




      public /*property*/string ResultIcon {
      // ------------------------------------------------------------------------
         get {
            if (OrigResult == EResult.NotPlayed) {
               switch (Override) {
                  case EResult.HomeWin: 
                  case EResult.VisWin: 
                  // Game was overriden by user...
                     return "result_override3.png";
                  case EResult.NotPlayed:
                  // Game is not played and not overriden, so will be simulated...
                     return "result_simulate4.png";
                  default: return "";
               }

            }
            else
            // Game was actually played...
               return "result_actual3.png";
         }
      }


      public /*Constructor*/ CGame(CTeam vis, CTeam home, ESite site) {
      // -----------------------------------------------------------------
      // Constructor:
         this.TeamH = home;
         this.TeamV = vis;
         this.Site = site;
         this.Spread = null; // PlayGame() will use power ratings
         ComputeHomeProb();

      }


      public /*constructor*/ CGame(string[] arec, int i) {
      // --------------------------------------------------
      // Constructor:
         int wk = int.Parse(arec[0]);
         string tagV = arec[1];
         string tagH = arec[2];

         char res = arec[3][0];
         int scoreV = res == 'P' ? int.Parse(arec[4]) : 0;
         int scoreH = res == 'P' ? int.Parse(arec[5]) : 0;
         ESite site = arec[3].Length > 1 && arec[3][1] == '*' ? ESite.Nuetral : ESite.Home;

         double? spread;
         if (res == 'N' && arec.Length > 4) spread = double.Parse(arec[4]);
         else spread = null;

         WeekNo = wk;
         GameNumber = i;
         TeamIdV = CTeam.teamIndexes[tagV];
         TeamIdH = CTeam.teamIndexes[tagH];
         ScoreV = scoreV;
         ScoreH = scoreH;
         Site = site;
         Spread = spread;

         TeamV = CSeason.Teams[TeamIdV];
         TeamH = CSeason.Teams[TeamIdH];
         switch (res) { 
            case 'P':
               if (scoreV > scoreH) Result = EResult.VisWin;
               else if (scoreV < scoreH) Result = EResult.HomeWin;
               else Result = EResult.Tie;
               OrigResult = Result;
               switch (Result) {
                  case EResult.HomeWin:
                     BumpRecs(TeamH, TeamV, false, EMode.Real);
                     Winner = TeamH; Loser = TeamV; WasTie = false;
                     break;
                  case EResult.VisWin:
                     BumpRecs(TeamV, TeamH, false, EMode.Real);
                     Winner = TeamV; Loser = TeamH; WasTie = false;
                     break;
                  case EResult.Tie:
                     BumpRecs(TeamV, TeamH, true, EMode.Real);
                     Winner = null; Loser = null; WasTie = true;
                     break;
               }
               break;
            case 'N':
               Result = EResult.NotPlayed;
               OrigResult = EResult.NotPlayed;
               ComputeHomeProb();
               break;
         }


      }


      private /*method*/ void ComputeHomeProb() {
      /* -------------------------------------------------
       * Base point spread on what's in schedule if any,
       * otherwise use diff in power ratings...
       * -----------------------------------------------*/
         if (!this.Spread.HasValue) { 
            Spread = TeamH.powerRating - TeamV.powerRating;
            if (Site != ESite.Nuetral) Spread += CSeason.HomeAdvantage;
         }
      // Convert point spread to a probability...
         this.HomeProb = CSeason.spreadTable.GetHomeProb(Spread.Value);
         //this.HomeProb = 0.5;

      }


      public void PlayGame(bool noTies=false) {
         /* -----------------------------------------------
          * This sets the following:
          * Winner, Losr, WasTie, Result (EResult)
          * nTies=true is for post-season
          * ----------------------------------------------- */


            // First, check if the game is overriden... (-bc 1807.01)
            switch (this.Override) {
               case EResult.NotPlayed:
                  // This means not overriden, take no action here.
                  break;
               case EResult.HomeWin:
                  Winner = TeamH; Loser = TeamV; WasTie = false;
                  Result = EResult.HomeWin;
                  //Debug.WriteLine(WeekNo.ToString() + " Override: " + TeamH.teamTag + " / " + TeamV.teamTag);
                  return;
               case EResult.VisWin:
                  Winner = TeamV; Loser = TeamH; WasTie = false;
                  Result = EResult.VisWin;
                  //Debug.WriteLine(WeekNo.ToString() + " Override: " + TeamV.teamTag + " / " + TeamH.teamTag);
                  return;
               case EResult.Tie:
                  // Currently app does not allow override as tie, but maybe future...
                  Winner = null; Loser = null; WasTie = true;
                  Result = EResult.Tie;
                  //Debug.WriteLine(WeekNo.ToString() + " Override: " + TeamV.teamTag + " ties " + TeamH.teamTag);
               return;
            }

            // Game is not overriden, so roll the dice...
            double tieProb = (noTies ? 0.0 : 0.01);
            double r = CSeason.rn.NextDouble(); //Roll the dice...
            if (r < tieProb) {
               // Tie game...
               Winner = null; Loser = null; WasTie = true;
               //BumpRecs(TeamH, TeamV, true, EMode.Sim);
               Result = EResult.Tie;
               //Debug.WriteLine(WeekNo.ToString() + " Simulated: " + TeamV.teamTag + " ties " + TeamH.teamTag);
            }
            else if (r < HomeProb + (1.0 - HomeProb) * tieProb) { //tieProb + prob*(1- tieProb))
            // Home team wins...
               Winner = TeamH; Loser = TeamV; WasTie = false;
               //BumpRecs(TeamH, TeamV, false, EMode.Sim);
               Result = EResult.HomeWin;
               //Debug.WriteLine(WeekNo.ToString() + " Simulated: " + TeamH.teamTag + " / " + TeamV.teamTag);
            }
            else {
            // Vis team wins...
               Winner = TeamV; Loser = TeamH; WasTie = false;
               //BumpRecs(TeamV, TeamH, false, EMode.Sim);
               Result = EResult.VisWin;
               //Debug.WriteLine(WeekNo.ToString() + " Simulated: " + TeamV.teamTag + " / " + TeamH.teamTag);
         }

      }

 
      public void BumpRecs(
         CTeam winner, CTeam loser, bool tie, EMode mode) {
         // ---------------------------------------------------------------
         bool isDiv = TeamV.divName == TeamH.divName;
         bool isConf = TeamV.confName == TeamH.confName;
         if (!tie)
            switch (mode) {
               case EMode.Real:
                  winner.realRec.league.w++; loser.realRec.league.l++;
                  if (isDiv) { winner.realRec.div.w++; loser.realRec.div.l++; }
                  if (isConf) { winner.realRec.conf.w++; loser.realRec.conf.l++; }
                  break;
               case EMode.Sim:
                  winner.simRec.league.w++; loser.simRec.league.l++;
                  if (isDiv) { winner.simRec.div.w++; loser.simRec.div.l++; }
                  if (isConf) { winner.simRec.conf.w++; loser.simRec.conf.l++; }
                  break;
            }
         else //tie
            switch (mode) {
               case EMode.Real:
                  TeamV.realRec.league.t++; TeamH.realRec.league.t++;
                  if (isDiv) { TeamV.realRec.div.t++; TeamH.realRec.div.t++; }
                  if (isConf) { TeamV.realRec.conf.t++; TeamH.realRec.conf.t++; }
                  break;
               case EMode.Sim:
                  TeamV.simRec.league.t++; TeamH.simRec.league.t++;
                  if (isDiv) { TeamV.simRec.div.t++; TeamH.simRec.div.t++; }
                  if (isConf) { TeamV.simRec.conf.t++; TeamH.simRec.conf.t++; }
                  break;
            }
      }


      private double SpreadToProb(double spread) {
         // ------------------------------------------------------
         // This sreturns the probability of the home team winning;
         // A positive spread means home team favored.

         // This is just placeholder logic for now...
         if (spread < -10) return .1;
         if (spread < -5) return .3;
         if (spread < -1) return .4;
         if (spread < 1) return .5;
         if (spread < 5) return .6;
         if (spread < 10) return .7;
         return .9;

      }


      public CTeam Opp(CTeam t) {
      // -------------------------------------------------------------
         if (t == this.TeamH) return this.TeamV;
         if (t == this.TeamV) return this.TeamH;
         return null;

      }


      public bool Involves(CTeam t) {
         // -------------------------------------------------------------
         return (this.TeamH == t || this.TeamV == t);

      }

   }
}

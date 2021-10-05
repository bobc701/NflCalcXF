using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Security;
using System.Security.Authentication;
using NflCalcXF.Services;

namespace NflCalc {

   public class CConference {
   // -----------------------------------------------------------
      public string ConferenceName;
      public Dictionary<string, CDivision> divs = new Dictionary<string, CDivision>();
      public CTeam[] seeds = new CTeam[8]; //We ignore 0, use 1..7

      public CGame[] Round1 = new CGame[3];
      public CGame[] Round2 = new CGame[2];
      public CGame Round3;

   }


   public class CDivision {
   // ---------------------------------------------------------
      public string DivisionName;
      public List<CTeam> teams = new List<CTeam>();
      public int MaxIx = 0; // Highest index that has made the playoffs thus far in that calcs.
      //public CTeam[] standings = new CTeam[5];

   }

   public class CSeason {
   // --------------------------------------------------------------
      public bool HasData { get; set; } = false; //New 6'18
      public DateTime DataDate { get; set; }     //New 6'18

      private const int tms = 32;
      private const int wks = 18;
      public CGame[] regSeason = new CGame[272];
      public CGame[][] postSeason = { new CGame[4], new CGame[4], new CGame[2], new CGame[1] };
      public static CTeam[] Teams = new CTeam[32];
      public static Dictionary<string, CConference> confs = new Dictionary<string, CConference>();

      public static System.Random rn; // = new System.Random();
      public CGame SuperBowl;
      private bool detailMode = false; 
      public static CSpreadTable1 spreadTable = new CSpreadTable1();
      public StringBuilder resultsString; 
      string resultsStringTemplate;
      public static double HomeAdvantage;


      public async Task<DateTime> GetDataDate() {
      // --------------------------------------------------------
         try {
            string s;
            StringReader sr = await DataAccess.GetTextFileOnLine("DataDate");
            if (sr == null) return DateTime.MinValue;

            s = sr.ReadLine();
            sr.Close();
            return DateTime.Parse(s);
         }
         catch (Exception ex) {
            return DateTime.MinValue;
         }
      }


      public async Task<bool> FillSpreadTable() {
      // --------------------------------------------------------
         try {
            StringReader srdr = await DataAccess.GetTextFileOnLine("Spread");
            if (srdr == null) return false;

            spreadTable.ReadSpreadTable(srdr);
            Debug.Write(System.Environment.UserName);
            srdr.Close();
            return true;
         }
         catch (Exception ex) {
            return false;
         }

      }


      public async Task<bool> FillResultsTemplate() {
         // --------------------------------------------------------
         try {
            StringReader srdr = await DataAccess.GetTextFileOnLine("Results");
            if (srdr == null) return false;

            resultsStringTemplate = srdr.ReadToEnd();
            resultsString = new StringBuilder();
            srdr.Close();
            return true;
         }
         catch {
            return false;
         }

      }


      public async Task<bool> FillSchedule() {
         // -----------------------------------------------------------
         try {
            StringReader sr = await DataAccess.GetTextFileOnLine("Schedule");
            if (sr == null) return false;

            confs.Clear();
            confs.Add("NFC", new CConference { ConferenceName = "NFC" });
            confs.Add("AFC", new CConference { ConferenceName = "AFC" });
            confs["NFC"].divs.Add("NFC-E", new CDivision { DivisionName = "NFC-E" });
            confs["NFC"].divs.Add("NFC-N", new CDivision { DivisionName = "NFC-N" });
            confs["NFC"].divs.Add("NFC-S", new CDivision { DivisionName = "NFC-S" });
            confs["NFC"].divs.Add("NFC-W", new CDivision { DivisionName = "NFC-W" });
            confs["AFC"].divs.Add("AFC-E", new CDivision { DivisionName = "AFC-E" });
            confs["AFC"].divs.Add("AFC-N", new CDivision { DivisionName = "AFC-N" });
            confs["AFC"].divs.Add("AFC-S", new CDivision { DivisionName = "AFC-S" });
            confs["AFC"].divs.Add("AFC-W", new CDivision { DivisionName = "AFC-W" });


            // Step 1. Read the teams...
            int i = 0;
            string rec = null;
            string[] arec;
            CTeam.teamIndexes = new Dictionary<string, short>();

            rec = sr.ReadLine();
            HomeAdvantage = double.Parse(rec.Split(new char[] { ' ' })[1]);
            while ((rec = sr.ReadLine()) != "") {
               arec = rec.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
               short ix = short.Parse(arec[0]); ix--;
               if (ix > 31) throw new Exception("More than 32 teams!");
               string tag = arec[1];
               string div = arec[2];
               string conf = div.Substring(0, 3);
               double rtg = double.Parse(arec[3]);
               CSeason.Teams[ix] = new CTeam(ix, tag, conf, div, rtg);
               CTeam.teamIndexes.Add(tag, ix);
               confs[conf].divs[div].teams.Add(Teams[ix]);

            }

            // Step 2. Read the games, past & future...
            i = 0;
            while ((rec = sr.ReadLine()) != null) {
               if (rec.Trim() == "") continue;
               arec = rec.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
               CGame g = new CGame(arec, i);
               g.TeamV.teamSchedule.Add(g);
               g.TeamH.teamSchedule.Add(g);
               regSeason[i] = g;
               i++;
            }
            return true;

         }
         catch {
            return false;

         }

      }


      public async Task<int> FetchData() {
         // ------------------------------------------------------------
         // Return values:
         //   0: Success, continue
         //  -1: Failure, do not continue
         //   1: Could not read date, but continue anyway.
         // ------------------------------------------------------------
         if (!this.HasData) {
         // There is no data... so read all the files...
            try {
               DateTime dtm = await GetDataDate();
               if (dtm == DateTime.MinValue) return -1;
               if (!await FillSpreadTable()) return -1;
               if (!await FillResultsTemplate()) return -1;
               if (!await FillSchedule()) return -1; ;
               this.HasData = true;
               this.DataDate = dtm;
               return 0;
            }
            catch (Exception ex) {
               return -1;
            }
         }
         else {
         // There *is* already data... Check it for currency...
            DateTime dtm;
            try {
               dtm = await GetDataDate();
               if (dtm == DateTime.MinValue) return 1;
            }
            catch (Exception ex) {
               return 1;
            }
            if (this.DataDate < dtm) {
               // There is data, but schedule is out of date...
               try {
                  if (!await FillSchedule()) return -1;
                  this.DataDate = dtm; 
                  return 0;
               }
               catch (Exception ex) {
                  this.HasData = false; //If couldn't read schedule, say no data...
                  return -1;
               }
            }
            else
               return 0;
         }

      }
            

      public void ShowSchedule() {
      // --------------------------------------------------------
         foreach (CGame g in regSeason) {
            Debug.WriteLine(
               g.GameNumber.ToString() + ", " +
               g.WeekNo + ", " + g.TeamV.teamTag + " at " + g.TeamH.teamTag);
         }
      }


      public void ShowRealStandings() {
      // -------------------------------------------------------
         string prior = "";
         foreach (CTeam t in CSeason.Teams.OrderBy(t => t.divName).ThenBy(t => 1.0-t.realRec.league.pct)) {
            if (t.divName != prior && prior != "") Debug.WriteLine("");
            Debug.WriteLine(
               "{0:6} {1} Lg: {2:##0} {3:##0} {4:#0} Conf:{5:##0} {6:##0} {7:#0} Div: {8:##0} {9:##0} {10:#0}",
               t.divName, String.Format("{0,-4}", t.teamTag),
               t.realRec.league.w, t.realRec.league.l, t.realRec.league.t,
               t.realRec.conf.w, t.realRec.conf.l, t.realRec.conf.t,
               t.realRec.div.w, t.realRec.div.l, t.realRec.div.t);
            prior = t.divName;
         }
      }

      
      public void ShowSimStandings() {
      // -------------------------------------------------------
         if (detailMode) {
         Debug.WriteLine("");
            string prior = "";
            foreach (CTeam t in CSeason.Teams.OrderBy(t => t.divName).ThenBy(t => 1.0 - t.simRec.league.pct)) {
               if (t.divName != prior && prior != "") Debug.WriteLine("");
               Debug.WriteLine(
                  "{0:6} {1} Lg: {2:##0} {3:##0} {4:#0} Conf:{5:##0} {6:##0} {7:#0} Div: {8:##0} {9:##0} {10:#0}",
                  t.divName, String.Format("{0,-4}", t.teamTag),
                  t.simRec.league.w, t.simRec.league.l, t.simRec.league.t,
                  t.simRec.conf.w, t.simRec.conf.l, t.simRec.conf.t,
                  t.simRec.div.w, t.simRec.div.l, t.simRec.div.t);
               prior = t.divName;
            }
         }
      }


      public void PlaySeason(bool debug = false) {
      // ----------------------------------------------------
         detailMode = debug;
         if (rn == null) rn = new System.Random();
         foreach (CTeam t in CSeason.Teams) t.simRec = t.realRec;
         foreach (CGame g in regSeason.Where(g => g.OrigResult == EResult.NotPlayed)) {
            g.PlayGame();
            g.BumpRecs(g.Winner, g.Loser, g.WasTie, EMode.Sim);
         }
         //ShowSimStandings();
         GetSeedings();
         PlayPostSeason();
      }


      public event Action<int> SeasonComplete;

      public int Simulate() {
         // ----------------------------------------------------------------------
         foreach (CTeam t in CSeason.Teams) t.tots.Zero();
         for (int trial=1; trial<=1000; trial++) {
            PlaySeason(debug:false);
            //Debug.Write(trial.ToString() + ' ');
            if (trial % 50 == 0) {
               //Debug.WriteLine("trial:" + trial.ToString());
               Xamarin.Forms.Device.BeginInvokeOnMainThread(() => { //--1908.01 (See N or EN)
                  SeasonComplete(trial);
               });
            }
         }

         ShowSimulatedResults();
         return 1000;
      }


      public void ShowSimulatedResults() {
      // -----------------------------------------------------------
      // This operates on the class-level StringBuilder, resultsString.
      // -------------------------------------------------------------
         string prior = "";
         //Debug.WriteLine("");
         //Debug.WriteLine( "Div   Team   w  l  t  POffs   Div  Conf    SB");
         //foreach (CTeam t in CSeason.Teams.OrderBy(t => t.divName).ThenByDescending(t => t.simRec.league.pct)) {
         //   if (t.divName != prior && prior != "") Debug.WriteLine("");
            
         //   Debug.WriteLine(
         //      "{0,-6}{1,-5}{2,3}{3,3}{4,3}{5,7:0.0}{6,6:0.0}{7,6:0.0}{8,6:0.0}",
         //      t.divName, t.teamTag,
         //      t.realRec.league.w, t.realRec.league.l, t.realRec.league.t,
         //      0.1*t.tots.Playoffs, 0.1*t.tots.Div, 0.1*t.tots.Conf, 0.1*t.tots.SB);
         //   prior = t.divName;
         //}

         resultsString.Clear();
         StringWriter sw = new StringWriter(resultsString);
         string hdr1 = "Divxx w-l-t   POff   Div  Conf   SB";
         string hdr2 = "-----------------------------------";
         //sw.WriteLine("");
         //sw.WriteLine("Div   Team   w  l  t  POffs   Div  Conf    SB");

         foreach (CTeam t in CSeason.Teams.OrderBy(t => t.divName).ThenByDescending(t => t.realRec.league.pct)) {
            if (t.divName != prior) { 
               sw.WriteLine("");
               sw.WriteLine(hdr1.Replace("Divxx", t.divName));
               sw.WriteLine(hdr2);
            }

            //--1910.01
            //sw.WriteLine(
            //   " {0,-4}{1,3}{2,3}{3,3}{4,6:0.0}{5,6:0.0}{6,6:0.0}{7,6:0.0}", //--1910.01
            //   t.teamTag,
            //   t.realRec.league.w, t.realRec.league.l, t.realRec.league.t,
            //   0.1 * t.tots.Playoffs, 0.1 * t.tots.Div, 0.1 * t.tots.Conf, 0.1 * t.tots.SB);

            Record rec = t.realRec.league;
            sw.WriteLine(" " + 
               $"{t.teamTag,-5}" +
               $"{rec.w + "-" + rec.l + "-" + rec.t,-6}" +
               $"{0.1 * t.tots.Playoffs,6:0.0}" +
               $"{0.1 * t.tots.Div,6:0.0}" +
               $"{0.1 * t.tots.Conf,6:0.0}" +
               $"{0.1 * t.tots.SB,5:0.0}"
            );
            prior = t.divName;
         }

      }


      public void GetSeedings() { 
      // ------------------------------------------------------
         CStandingsGroup rg;

         resultsString.Clear();
         resultsString.Append(resultsStringTemplate);

         foreach (var c in confs.Values) { 
            //ShowDetail("Conference: " + c.ConferenceName);

            List<CTeam> teamList = new List<CTeam>();
            foreach (var d in c.divs.Values) {
               rg = new CStandingsGroup(d.teams, detailMode);
               d.teams = rg.RankTeams('d');
               if (detailMode)  
                  resultsString.Replace("{" + d.DivisionName + "}", rg.StandingsString(new string[] { "<-- Div winner", "", "", "" })); //--1910.01
            }

         // Each division's standings are set.
         // So now sort the div winners....
            foreach (var d in c.divs.Values) 
            {  teamList.Add(d.teams[0]); //Add the div winner
            }
            rg = new CStandingsGroup(teamList, detailMode);
            teamList = rg.RankTeams('c');
            if (detailMode) {
               resultsString.Replace("{" + c.ConferenceName + "-DIV}",
                  rg.StandingsString(new string[] { "<-- 1st seed", "<-- 2nd seed", "<-- 3rd seed", "<-- 4th seed" }));
            }

         // -------------------------------------------
         // Now drop the div winners into sseeds 1 to 4
         // ------------------------------------------
            c.seeds[1] = teamList[0]; c.seeds[1].tots.Div++;
            c.seeds[2] = teamList[1]; c.seeds[2].tots.Div++;
            c.seeds[3] = teamList[2]; c.seeds[3].tots.Div++;
            c.seeds[4] = teamList[3]; c.seeds[4].tots.Div++;

            //// For the 14 team post-season, we need a new concept, which keeps
            //// track of the lowest ranking (highest index) team in each div that 
            //// has made the playoffs. So initially, all 4 dives will be = 1 (2nd place).
            //var maxix = new Dictionary<string, int>();
            //foreach (CDivision d in c.divs.Values) {
            //   maxix.Add(d.DivisionName, 0);
            //}

         // ----------------------------------------
         // Now, Get #5 seed among the 4 2nd placers
         // ----------------------------------------
            teamList.Clear();
            foreach (var d in c.divs.Values) {
               teamList.Add(d.teams[1]); //Add second place teams
               d.MaxIx = 0;
            }
            rg = new CStandingsGroup(teamList, detailMode);
            teamList = rg.RankTeams('c');
            c.seeds[5] = teamList[0];    //Assign seed #5
         // Increment max index of teams in this div that have made postseason so far.
            int n = ++c.divs[teamList[0].divName].MaxIx;

            if (detailMode) {
               resultsString.Replace("{" + c.ConferenceName + "-5}", 
                  rg.StandingsString(new string[] { "<-- 5th seed", "", "", "" }));
            }

         // --------------------------------------------------------
         // Next, get #6 seed from 3 second places and one 3rd place
         // --------------------------------------------------------
            teamList.Clear();
            foreach (var d in c.divs.Values) {
               teamList.Add(d.teams[d.MaxIx+1]); //Will be 3-2nd place and 1-3rd place team
            }

            rg = new CStandingsGroup(teamList, detailMode);
            teamList = rg.RankTeams('c');
            c.seeds[6] = teamList[0]; //Assign seed #6
         // Increment max index of teams in this div that have made postseason so far.
         // At this point it will be like 1,1,0,0 or like 2,0,0,0.
            c.divs[teamList[0].divName].MaxIx++;

            if (detailMode) {
               resultsString.Replace("{" + c.ConferenceName + "-6}", 
               rg.StandingsString(new string[] { "<-- 6th seed", "", "", "" }));
            }

         // --------------------
         // Finally, get #7 seed
         // --------------------
         // Note that d.Teams is sorted by standing (0..3)
            teamList.Clear();
            foreach (var d in c.divs.Values) {
               teamList.Add(d.teams[d.MaxIx+1]); //Could be 2nd or 3rd or even 4th place teams
            }
            rg = new CStandingsGroup(teamList, detailMode);
            teamList = rg.RankTeams('c');
            c.seeds[7] = teamList[0]; //Assign seed #7

            if (detailMode) {
               resultsString.Replace("{" + c.ConferenceName + "-7}",
               rg.StandingsString(new string[] { "<-- 7th seed", "", "", "" }));
            }

         // ------------------------------------------
         // Store the seed number with the team irself
         // ------------------------------------------
         // This will be needed in the post-season for reseeding.
            var rec = new StringBuilder();
            for (int s = 1; s <= 7; s++) { 
               c.seeds[s].Seed = s; 
               c.seeds[s].tots.Playoffs++; 
               rec.AppendLine(string.Format(" {0,1}. {1,-3}", s, c.seeds[s].teamTag));
            }
            resultsString.Replace("{" + c.ConferenceName + "-SEEDS}", rec.ToString());
            if (detailMode) { 
               Debug.WriteLine("");
               Debug.WriteLine("Seeds:");
               for (int s = 1; s <= 7; s++) Debug.WriteLine(s.ToString() + ". " + c.seeds[s].teamTag);
            }
         }

      }

      private void ResultsReplace(string token, string value) {
      // -----------------------------------------------------------------
         resultsString.Replace(token, value);
      }

      public void PlayPostSeason() {
      // -------------------------------------------------

      // Round 1 (Wildcard round)...

         foreach (CConference conf in confs.Values) {
            conf.Round1[0] = new CGame(conf.seeds[7], conf.seeds[2], ESite.Home);
            conf.Round1[1] = new CGame(conf.seeds[6], conf.seeds[3], ESite.Home);
            conf.Round1[2] = new CGame(conf.seeds[5], conf.seeds[4], ESite.Home);

            conf.Round1[0].PlayGame(true);
            conf.Round1[1].PlayGame(true);
            conf.Round1[2].PlayGame(true);

         // Round 2 (Divisional round)...

            List<CTeam> teams = new List<CTeam>();
            teams.Add(conf.Round1[0].Winner);
            teams.Add(conf.Round1[1].Winner);
            teams.Add(conf.Round1[2].Winner);
            teams = teams.OrderBy(t => t.Seed).ToList<CTeam>(); //Reseed the round 1 winners

            conf.Round2[0] = new CGame(teams[1], teams[0], ESite.Home);
            conf.Round2[1] = new CGame(teams[2], conf.seeds[1], ESite.Home); //#1 seed makes appearance

            conf.Round2[0].PlayGame(true);
            conf.Round2[1].PlayGame(true);

         // Round 3 (Conf Championships)...

            teams.Clear();
            teams.Add(conf.Round2[0].Winner); 
            teams.Add(conf.Round2[1].Winner); 

            teams = teams.OrderBy(t => t.Seed).ToList<CTeam>(); ;

            conf.Round3 = new CGame(teams[1], teams[0], ESite.Home);
            conf.Round3.PlayGame(true);
            conf.Round3.Winner.tots.Conf++;
         }

      // Round 4 (Super Bowl)...
      // Don't worry about home / visitor, since nuetral site...

         SuperBowl = new NflCalc.CGame(
            confs["AFC"].Round3.Winner, confs["NFC"].Round3.Winner, ESite.Nuetral);
         SuperBowl.PlayGame(true);
         SuperBowl.Winner.tots.SB++;


         if (detailMode) {
         // Format the results for single-season display...

            var s = new StringBuilder();
            s.AppendLine(" " +
               confs["NFC"].Round1[0].Winner.teamTag + " defeats " +
               confs["NFC"].Round1[0].Loser.teamTag);
            s.AppendLine(" " +
               confs["NFC"].Round1[1].Winner.teamTag + " defeats " +
               confs["NFC"].Round1[1].Loser.teamTag);
            s.AppendLine(" " +
               confs["NFC"].Round1[2].Winner.teamTag + " defeats " +
               confs["NFC"].Round1[2].Loser.teamTag);
            s.AppendLine(" " +
               confs["AFC"].Round1[0].Winner.teamTag + " defeats " +
               confs["AFC"].Round1[0].Loser.teamTag);
            s.AppendLine(" " +
               confs["AFC"].Round1[1].Winner.teamTag + " defeats " +
               confs["AFC"].Round1[1].Loser.teamTag);
            s.AppendLine(" " +
               confs["AFC"].Round1[2].Winner.teamTag + " defeats " +
               confs["AFC"].Round1[2].Loser.teamTag);
            resultsString.Replace("{ROUND-1}", s.ToString());

            s.Clear();
            s.AppendLine(" " +
               confs["NFC"].Round2[0].Winner.teamTag + " defeats " +
               confs["NFC"].Round2[0].Loser.teamTag);
            s.AppendLine(" " +
               confs["NFC"].Round2[1].Winner.teamTag + " defeats " +
               confs["NFC"].Round2[1].Loser.teamTag);
            s.AppendLine(" " +
               confs["AFC"].Round2[0].Winner.teamTag + " defeats " +
               confs["AFC"].Round2[0].Loser.teamTag);
            s.AppendLine(" " +
               confs["AFC"].Round2[1].Winner.teamTag + " defeats " +
               confs["AFC"].Round2[1].Loser.teamTag);
            resultsString.Replace("{ROUND-2}", s.ToString());

            s.Clear();
            s.AppendLine(" " +
               confs["NFC"].Round3.Winner.teamTag + " defeats " +
               confs["NFC"].Round3.Loser.teamTag);
            s.AppendLine(" " +
               confs["AFC"].Round3.Winner.teamTag + " defeats " +
               confs["AFC"].Round3.Loser.teamTag);
            resultsString.Replace("{ROUND-3}", s.ToString());

            s.Clear();
            s.AppendLine(" " +
               SuperBowl.Winner.teamTag + " defeats " +
               SuperBowl.Loser.teamTag);
            resultsString.Replace("{ROUND-SB}", s.ToString());

         }

      }



      private void DebugRankings(List<CTeam> teamList, string tag) {
      // -----------------------------------------------------------------

         if (detailMode) {
            Debug.WriteLine("");
            Debug.WriteLine(tag);
            foreach (CTeam t in teamList) Debug.WriteLine(t.teamTag + " " + t.Metric);
         }




      }




   }

}


   


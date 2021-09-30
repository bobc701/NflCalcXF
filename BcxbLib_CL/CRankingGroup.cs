using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NflCalc.ExtentionMethods;

namespace NflCalc {

   class CStandingsGroup {

      private List<CTeam> teams;
      private bool detailMode;
      private string token;
      private StringBuilder tieBreakerSummary = new StringBuilder();

      public string TieBreakerString { 
      // ---------------------------------------------------------
         get {  
            if (tieBreakerSummary.Length == 0) return "";
            else return "Tie-breakers:\r\n" + tieBreakerSummary.ToString();
         }
      }


      public string StandingsString (string[] comments) {
      //-------------------------------------------------------  
         int i = 1;
         var s = new StringBuilder();
         foreach (var t in teams) {  
            s.AppendLine(
               string.Format(" {0,1}. {1,-3}  {2}  {3}", i, t.teamTag, t.simRec.league.wltStringFix, comments[i-1]));
               i++;
            }
         if (tieBreakerSummary.Length > 0) { 
            s.AppendLine();
            s.AppendLine(" Tiebreakers:");
            s.AppendLine(tieBreakerSummary.ToString());
         }
         return s.ToString();
      }


      public CStandingsGroup(List<CTeam> teams1, bool detailMode1, string token1 = null) {
      // ----------------------------------------------------------------------------
         teams = teams1;
         detailMode = detailMode1;
         token = token1;
         if (token1 != null) token1 = "{" + token1 + "}";

      }


      public List<CTeam> RankTeams(char mode) {
      // -------------------------------------------------------------------------
         foreach (var t in teams) t.Metric = t.simRec.league.spct;
         RankTeamsByMetric(ref teams);
         var ties = GetTies(teams);
         tieBreakerSummary.Clear();
         while (ties != null) {
            BreakTies2div(ties, mode);
            RankTeamsByMetric(ref teams);
            ties = GetTies(teams);
         }

      // All ties have been broken...
         return teams;

      }


      private void BreakTies2div(List<CTeam> ties, char mode) {
      // -----------------------------------------------------------------
      // From http://www.nfl.com/standings/tiebreakingprocedures
      // 1. Head - to - head (sweep)(best won - lost - tied percentage in games between the clubs).
      // 2. Best won - lost - tied percentage in games played within the division.
      // 3. Best won-lost - tied percentage in common games.
      // 4. Best won-lost - tied percentage in games played within the conference.
      // -----------------------------------------------------------------

         int orig = ties.Count;
         List<CTeam> ties1 = ties;
         //tieBreakerSummary.Clear();

      // Step 1. Head-to-head...
         Head2Head(ties1, mode);
         RankTeamsByMetric(ref ties1);
         ties1 = GetTies(ties1);
         if (ties1 == null || (orig > 2 && ties1.Count == 2)) return;

      // Games in division...
         if (mode == 'd') {
            GamesInDiv(ties1);
            RankTeamsByMetric(ref ties1);
            ties1 = GetTies(ties1);
            if (ties1 == null || (orig > 2 && ties1.Count == 2)) return;
         }

      // Games in conference (if conf breaker)...
         if (mode == 'c') {
            GamesInConf(ties1);
            RankTeamsByMetric(ref ties1);
            ties1 = GetTies(ties1);
            if (ties1 == null || (orig > 2 && ties1.Count == 2)) return;
         }

      // Common opponents...
         GamesVsCommonOpponents(ties1);
         RankTeamsByMetric(ref ties1);
         ties1 = GetTies(ties1);
         if (ties1 == null || (orig > 2 && ties1.Count == 2)) return;

      // Games in conference (if div breaker)...
         if (mode == 'd') {
            GamesInConf(ties1);
            RankTeamsByMetric(ref ties1);
            ties1 = GetTies(ties1);
            if (ties1 == null || (orig > 2 && ties1.Count == 2)) return;
         }

      // Strength of victory...
         StrengthOfVictory(ties1);
         RankTeamsByMetric(ref ties1);
         ties1 = GetTies(ties1);
         if (ties1 == null || (orig > 2 && ties1.Count == 2)) return;

      // Strength of schedule...
         StrengthOfSchedule(ties1);
         RankTeamsByMetric(ref ties1);
         ties1 = GetTies(ties1);
         if (ties1 == null || (orig > 2 && ties1.Count == 2)) return;

      // Not implemented... tie breakers involving points

      // Final step. Coin toss...
         CoinToss(ties1);
         RankTeamsByMetric(ref ties1);

      }


      private string Head2Head(List<CTeam> ties, char mode) {
      // -----------------------------------------------------------------------
      // Inside division, you will play each team in the tied group twice each,
      // and so you must have a win pct of 1.0 or 0.0, any thing else is middle ground.
      // Across div's, you will play ea team in the tied group 1 or 0 times. So you 
      // must win n games, where n is the number of teams in the group. QUESTION: there
      // may be other teams in the group, who don't play all the other teams, and so 
      // they can't win or lose the tie-breaker... or does that invalidate the entire 
      // step????
      // -----------------------------------------------------------------------

         Record rec; 
         StringBuilder msg;
         if (ties.Count > 2)
             msg = new StringBuilder(detailMode ? "Head-to-head sweep: " : "");
         else
             msg = new StringBuilder(detailMode ? "Head-to-head: " : "");

         foreach (CTeam t in ties) 
         {  rec = default(Record);
            foreach (CGame g in t.teamSchedule.Where(g => ties.Contains(g.Opp(t)))) 
            {  if (g.Winner == t) rec.w++;
               else if (g.WasTie) rec.t++;
               else rec.l++; 
            }
            char h2h = ' '; 
            switch (mode) 
            {  case 'd':
                  if (rec.w == 2 * (ties.Count - 1)) h2h = '2';
                  else if (rec.l == 2 * (ties.Count - 1)) h2h = '0';
                  else h2h = '1'; 
                  break;
               case 'c':
                  if (rec.w == ties.Count - 1) h2h = '2';
                  else if (rec.l == ties.Count - 1) h2h = '0';
                  else h2h = '1';
                  break;
            }
            t.Metric += "h" + h2h.ToString();

            if (detailMode) {
               msg.Append(
                  t.teamTag + ":" + rec.wltString + " "); //--1910.01
            }

         }
         if (detailMode) tieBreakerSummary.AppendLine(" " + msg.ToString());
         return msg.ToString();


      }


      private void GamesInDiv(List<CTeam> ties) {
      // ----------------------------------------------------
         StringBuilder msg = null;
         if (detailMode) msg = new StringBuilder("Games in div: ");
         foreach (CTeam t in ties) {
            t.Metric += 'd' + t.simRec.div.spct;
            if (detailMode) msg.Append(t.teamTag + ":" + t.simRec.div.wltString + " ");
         }
         if (detailMode) tieBreakerSummary.AppendLine(" " + msg.ToString());
      }


      private void GamesInConf(List<CTeam> ties) {
      // ----------------------------------------------------
         StringBuilder msg = new StringBuilder("Games in conf: ");
         foreach (CTeam t in ties) {
            t.Metric += 'c' + t.simRec.conf.spct;
            if (detailMode) msg.Append(t.teamTag + ":" + t.simRec.conf.wltString + " ");
         }
         if (detailMode) tieBreakerSummary.AppendLine(" " + msg.ToString());

      }


      private void GamesVsCommonOpponents(List<CTeam> ties) {
      /* -----------------------------------------------------------------------------------
       * Found this on Reddit... I think it's correct. The 4-minimum referes to the
       * number of common opps, not the number of games, which can differ...
       * 
       * It's based on winning percentage, and there must be at least 4 common opponents
       * for it to apply.
       * Consider a scenario where teams A and B are tied up to the common games tie-breaker. 
       * Team A played teams W, X, Y, and Z and had a record of 2-2. 
       * Team B played teams W, X, Y once each and team Z twice, with a record of 2-3.
       * In this case team A wins the tie-breaker.
       * ----------------------------------------------------------------------------------*/
         StringBuilder msg = new StringBuilder("Games vs common opps: ");
         List<CTeam> opps = CommonOpponents(ties);
         Record rec;

         if (opps.Count < 4) {
            foreach (var t in ties) t.Metric += "oXXX";
            if (detailMode) msg.Append("Less than 4 common opps");
         }
         else {
            foreach (var t in ties) {
               rec = default(Record);
               foreach (var g in t.teamSchedule) {
                  CTeam opp = g.Opp(t);
                  if (opps.Contains(opp) && !ties.Contains(opp)) {
                     if (g.Winner == t) rec.w++;
                     else if (g.WasTie) rec.t++;
                     else rec.l++;
                  }
               }
               t.Metric += 'o' + rec.spct;
               if (detailMode) msg.Append(t.teamTag + ":" + rec.wltString + " ");
            }
         }
         if (detailMode) tieBreakerSummary.AppendLine(" " + msg.ToString());

      }
      

      private void StrengthOfVictory(List<CTeam> ties) {
         /* -----------------------------------------------------------------------------------
          * Combined record of all opponents.
          * If you play a team twice, does it count twice. I say yes.
          * ----------------------------------------------------------------------------------*/
         var msg = new StringBuilder("Str. of victory: ");
         foreach (var t in ties) {
            Record rec = default(Record);
            foreach (var g in t.teamSchedule) {
               CTeam opp = g.Opp(t);
               if (g.Winner == t) rec += opp.simRec.league; //Check if t was winner + accum. opp's rec.
            }
            t.Metric += 'v' + rec.spct;
            if (detailMode) msg.Append(t.teamTag + ":" + rec.wltString + " ");
         }
         if (detailMode) tieBreakerSummary.AppendLine(" " + msg.ToString());

      }


      private void StrengthOfSchedule(List<CTeam> ties) {
         /* -----------------------------------------------------------------------------------
          * Combined record of all opponents.
          * If you play a team twice, does it count twice. I say yes.
          * ----------------------------------------------------------------------------------*/
         var msg = new StringBuilder("Str. of schedule: ");
         foreach (var t in ties) {
            Record rec = default(Record);
            foreach (var g in t.teamSchedule) {
               CTeam opp = g.Opp(t);
               rec += opp.simRec.league; //Do not check if t was winner,just accumulate opp's rec.
            }
            t.Metric += 's' + rec.spct;
            if (detailMode) msg.Append(t.teamTag + ":" + rec.wltString + " ");
         }
         if (detailMode) tieBreakerSummary.AppendLine(" " + msg.ToString());

      }


      private List<CTeam> CommonOpponents(List<CTeam> ties) {
      // -----------------------------------------------------
         List<CTeam> result = ties[0].Opponents;
         for (int i = 1; i < ties.Count; i++) {
            //result = result.FindAll(t => ties[i].Opponents.Contains(t));
            result = result.Intersect(ties[i].Opponents).ToList();
         }
         return result;

      }


      private void CoinToss(List<CTeam> ties) {
      // ----------------------------------------------------
         string displayVal(int n) {
            if (ties.Count == 2) return n == 0 ? "Lost" : "Won";
            else return (ties.Count-n).ToString();
         }



         StringBuilder msg = null;
         if (detailMode) 
            msg = new StringBuilder(ties.Count == 2 ? "Coin toss: " : "Draw lots (high wins): ");
         
         int[] a = GenIndexArray(0, ties.Count-1).RandomizeArray();
         int i = 0;
         foreach (CTeam t in ties) {
            t.Metric += "t" + a[i].ToString();
            if (detailMode) msg.Append(" " + t.teamTag + ":" + displayVal(a[i]) + " ");
            i++;
         }
         if (detailMode) tieBreakerSummary.AppendLine(" " + msg.ToString());

      }


      private void RankInDivision(CDivision d) {
      // -------------------------------------------------------------------------
      // Here we operate on the (4) teams in a given division, so just the
      // division itself is passed it.
      // When done, the division's teams willbe sorted by place,
      // -------------------------------------------------------------------------
         //if (_debug) {
         //   Debug.WriteLine("Rank teams in division: " + d.DivisionName);
         //}

         foreach (var t in d.teams) t.Metric = t.simRec.league.spct;
         RankTeamsByMetric(ref d.teams);
         var ties = GetTies(d.teams);
         tieBreakerSummary.Clear();
         while (ties != null) {
            BreakTies2div(ties, 'd');
            RankTeamsByMetric(ref teams);
            ties = GetTies(d.teams);
         }

         //string sDisplay = StandingsString(d) + "\r\n\r\n" + tieBreakerSummary.ToString();

      }


      private void RankInConference(ref List<CTeam> teams) {
      // -------------------------------------------------------------------------
      // This is used several cross-div scenarios, so caller is reponsible for 
      // supplying a pre-built team list.
      // -------------------------------------------------------------------------
            
         foreach (var t in teams) t.Metric = t.simRec.league.spct;
         RankTeamsByMetric(ref teams);
         var ties = GetTies(teams);
         while (ties != null) {
            BreakTies2div(ties, 'c');
            RankTeamsByMetric(ref teams);
            ties = GetTies(teams);
         }

         // All ties have been broken...
         RankTeamsByMetric(ref teams);

      }

      
      private void RankTeamsByMetric(ref List<CTeam> teams1) {
      // ---------------------------------------------------------
         teams1 = teams1.OrderByDescending(t => t.Metric).ToList<CTeam>();

      }


      private List<CTeam> GetTies(List<CTeam> teams) {
      // -------------------------------------------------------
         var ties = new List<CTeam>();
         for (int i = 0; i <= teams.Count - 2; i++) {
            string m = teams[i].Metric;
            ties = teams.Skip(i).TakeWhile(t => t.Metric == m).ToList<CTeam>();
            if (ties.Count >= 2) return ties;
         }
         return null;
      }




   }
}

using System;
using System.Collections.Generic;
using System.Text;
using NflCalc;
using System.IO;
using System.Reflection;
using System.Net.NetworkInformation;

using System.Net;
using System.Web;

//Temp...
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace NflCalcXF.Services {

   static class Repository {

      public static CSeason season { get; set; }

      public static void GetSeason() {
         // ------------------------------------------------------------------
         season = new CSeason();

      }

      public static string SiteUsed = "";


      private static void OpenTextFiles(out StreamReader f1, out StreamReader f2, out StreamReader f3) {
      // ----------------------------------------------------------------------------------------
      // I think this is obsolete. But it illustrates how to read local text file
      // ----------------------------------------------------------------------------------------
         var assembly = typeof(MainPage).GetTypeInfo().Assembly;
         Stream f;
         f = assembly.GetManifestResourceStream("NflCalcXF.Resources.SpreadTable3.txt");
         f1 = new StreamReader(f);
         f = assembly.GetManifestResourceStream("NflCalcXF.Resources.ResultsTemplate1.txt");
         f2 = new StreamReader(f);
         f = assembly.GetManifestResourceStream("NflCalcXF.Resources.Schedule2.txt");
         f3 = new StreamReader(f);

      }


      public static StreamReader GetTextFileOnDisk(string token) {
      // --------------------------------------------------------------------------------
         var assembly = typeof(MainPage).GetTypeInfo().Assembly;

         string path = "";
         switch (token) {
            case "help": path = @"NflCalcXF.Resources.Help1.txt"; break;
            case "about": path = @"NflCalcXF.Resources.About1.txt"; break;
         }

         Stream strm = assembly.GetManifestResourceStream(path);
         return new StreamReader(strm);

      }


      public static StreamReader GetTextFileOnLine(string token) {
      // ---------------------------------------------------------------
         //WebClient client = new WebClient(); 
         string path = "";
         Stream strm;

         //switch (token) {
         //   case "DataDate": path = @"http://www.zeemerix.com/NflData/DataDate1.txt"; break;
         //   case "Spread": path = @"http://www.zeemerix.com/NflData/SpreadTable3.txt"; break;
         //   case "Results": path = @"http://www.zeemerix.com/NflData/ResultsTemplate1.txt"; break;
         //   case "Schedule": path = @"http://www.zeemerix.com/NflData/Schedule2.txt"; break;
         //}

         // To enable C# 8, I put <LangVersion>8.0</LangVersion> in .csproj

         path = token switch {
            "DataDate" => @"http://www.zeemerix.com/NflData/DataDate1.txt",
            "Spread"   => @"http://www.zeemerix.com/NflData/SpreadTable3.txt",
            "Results"  => @"http://www.zeemerix.com/NflData/ResultsTemplate1.txt",
            "Schedule" => @"http://www.zeemerix.com/NflData/Schedule2.txt"
         };

         if (path.Contains("4bcx")) SiteUsed = "4bcx.com";
         else if (path.Contains("zeemerix")) SiteUsed = "zeemerix.com";
         else SiteUsed = "other";


         // ----------------------------------------------------
         // Found this approach on Web.
         // Uses HttpWebRequest i/o WebClient, and so allows you 
         // to set the timeout period.
         // HttpWebRequest --> HttpWebResponse --> Stream --> StreamReader
         // ----------------------------------------------------
         HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path); 
         request.Timeout = 30000;
         request.ReadWriteTimeout = 30000;
         //-bc 1807.01: using 'using' per StackOverflow...
         //using (var wresp = (HttpWebResponse)request.GetResponse()) { 
         //   strm = wresp.GetResponseStream();
         //}
         using (var wresp = (HttpWebResponse)request.GetResponse()) {
            strm = wresp.GetResponseStream();
         }
         //strm = client.OpenRead(path);
         return new StreamReader(strm);

      //   Here's what bcxbXf does:'
      //   var f = new StreamReader(resp.GetResponseStream());
      //   s = f.ReadToEnd();
      //}
      //   return new StringReader(s);

   }


   public static StreamReader GetTextFileLocal(string token) {
      // ---------------------------------------------------------------
      // Intended to be used for testing without going to the Web.
      // (I have not wired it in yet.)
      // ---------------------------------------------------------------
         string path = "";
         Stream strm;

         switch (token) {
            case "DataDate": path = @"http://www.zeemerix.com/NflData/DataDate1.txt"; break;
            case "Spread": path = @"http://www.zeemerix.com/NflData/SpreadTable3.txt"; break;
            case "Results": path = @"http://www.zeemerix.com/NflData/ResultsTemplate1.txt"; break;
            case "Schedule": path = @"http://www.zeemerix.com/NflData/Schedule2.txt"; break;
         }

         var assembly = typeof(MainPage).GetTypeInfo().Assembly;
         strm = assembly.GetManifestResourceStream(path);
         return new StreamReader(strm);

      }


   }

}

using System;
using System.Collections.Generic;
using System.Text;
using NflCalc;
using System.IO;
using System.Reflection;
using System.Net.NetworkInformation;

using System.Net;
using System.Web;

using Xamarin.Forms;

//Temp...
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace NflCalcXF.Services {

   static class Repository {

      public static CSeason season { get; set; }
      public static string SiteUsed { get; set; } = "internet";
      public static string ErrorMsg { get; set; } = "";

      public static HttpClient httpClient = new HttpClient() {
         Timeout = new TimeSpan(0, 0, 0, 30),
         DefaultRequestHeaders = {
            CacheControl = CacheControlHeaderValue.Parse("no-cache, no-store, must-revalidate"),
            Pragma = { NameValueHeaderValue.Parse("no-cache") }
         },
      };


      static Repository() {
      // ---------------------------------------
         httpClient.DefaultRequestHeaders.CacheControl.NoCache = true;
         httpClient.DefaultRequestHeaders.CacheControl.NoStore = true;

      }


      public static void GetSeason() {
         // ------------------------------------------------------------------
         season = new CSeason();

      }
      

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


      public async static Task<StringReader> GetTextFileOnLine(string token) {
      // ---------------------------------------------------------------
         //WebClient client = new WebClient(); 

      // To enable C# 8, I put <LangVersion>8.0</LangVersion> in .csproj
         string path = token switch {
            "DataDate" => @"https://www.zeemerixdata.com/NflData/DataDate1.txt",
            "Spread"   => @"https://www.zeemerixdata.com/NflData/SpreadTable3.txt",
            "Results"  => @"https://www.zeemerixdata.com/NflData/ResultsTemplate1.txt",
            "Schedule" => @"https://www.zeemerixdata.com/NflData/Schedule2.txt"
         };
         SiteUsed = "zeemerixdata.com";

         // how Amar_Bait said to do it on Xamarin Formums
         // ----------------------------------------------
         //var httpClient = new HttpClient(); // Good idea to re-use this, so make it global.
         //HttpResponseMessage response = await httpClient.GetAsync(path); // or .GetStringAsync(path)
         //response.EnsureSuccessStatusCode(); // To make sure our request was successful (i.e. >=200 and <400)
         //var s = await response.Content.ReadAsStringAsync(); // read the response body

         // Another way
         // -----------
         HttpResponseMessage response = null;
         try {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
               new MediaTypeWithQualityHeaderValue("application/text") //text /plain")
            );
            string s = "";                               
            response = await httpClient.GetAsync(path); 
            if (response.IsSuccessStatusCode) {
               s = await response.Content.ReadAsStringAsync(); //for .net std!
            }
            else {
               //throw new Exception($"Error getting {token} response from {SiteUsed}");
               ErrorMsg = $"Error: Response status code = {(int)response.StatusCode}";
               return null;
            }

            // Using HttpWebRequest
            // ----------------------------------------------
            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path); 
            //request.Timeout = 30000;
            //request.ReadWriteTimeout = 30000;
            //string s;
            //using (var wresp = (HttpWebResponse)request.GetResponse()) {
            //   var sr = new StreamReader(wresp.GetResponseStream());
            //   s = sr.ReadToEnd(); 
            //}

            ErrorMsg = "";
            return new StringReader(s);
         }
         catch (Exception ex) {
            ErrorMsg = ex.Message; // $"{ex.Message} (Status code:{(int)response.StatusCode})"; 
            return null;

         }
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

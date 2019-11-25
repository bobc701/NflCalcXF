using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NflCalcXF.Services;
using Foundation;



[assembly: Xamarin.Forms.Dependency(typeof(NflCalcXF.iOS.Version_iOS))]
namespace NflCalcXF.iOS {

   public class Version_iOS : IAppVersion
   {
      public string GetVersion() {
         return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
      }
      public int GetBuild() { 
         return int.Parse(NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString());
      }
   }
}
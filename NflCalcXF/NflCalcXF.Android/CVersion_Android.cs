using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content; using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using NflCalcXF.Services;


[assembly: Xamarin.Forms.Dependency(typeof(NflCalcXF.Droid.CVersion_Android))]
namespace NflCalcXF.Droid {

   class CVersion_Android : IAppVersion {

      public string GetVersion() {
         // ------------------------------------------------------
         var context = global::Android.App.Application.Context;
         PackageManager manager = context.PackageManager;
         PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);
         return info.VersionName;
      }

      public int GetBuild() {
         var context = global::Android.App.Application.Context;
         PackageManager manager = context.PackageManager;
         PackageInfo info = manager.GetPackageInfo(context.PackageName, 0);
         return info.VersionCode;
      }



   }
}

// This is for IOS when you do that...

//[assembly: Xamarin.Forms.Dependency(typeof(Your.Namespace.iOS.Version_iOS))]
//namespace Your.Namespace.iOS {
//   public class Version_iOS : IAppVersion {
//      public string GetVersion() {
//         return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
//      }
//      public int GetBuild() {
//         return int.Parse(NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString());
//      }
//   }
//}
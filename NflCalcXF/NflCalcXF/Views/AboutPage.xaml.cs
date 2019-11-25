//#define LITE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using static NflCalcXF.Services.Repository;
using NflCalcXF.Services;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NflCalcXF.Views
{
	
	public partial class AboutPage : ContentPage
	{
		public AboutPage ()
		{
			InitializeComponent ();
         this.Title = "About Pro Football What-If";
#if LITE
         this.imgAbout.Source = "SplashLite1.png";
#else
         this.imgAbout.Source = "Splash1.png";
#endif
      // Logic here to read help file, help.txt, and display.
         string v =
            "Version: " + DependencyService.Get<IAppVersion>().GetVersion() +
            " Build: " + DependencyService.Get<IAppVersion>().GetBuild().ToString();
         using (StreamReader f = GetTextFileOnDisk("about")) {
            this.lblAbout.Text = f.ReadToEnd().Replace("[android_version]", v);
            f.Close();
         }
      }
   }
}
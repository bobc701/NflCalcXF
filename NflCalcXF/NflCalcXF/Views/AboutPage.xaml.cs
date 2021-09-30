//#define LITE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
         this.imgAbout.Source = "Splash1.png";

      // Logic here to read help file, help.txt, and display.
         string v =
            "Version: " + DependencyService.Get<IAppVersion>().GetVersion() +
            " Build: " + DependencyService.Get<IAppVersion>().GetBuild().ToString();
         using (StreamReader f = Repository1.GetTextFileOnDisk("about")) {
            this.lblAbout.Text = f.ReadToEnd().Replace("[android_version]", v);
            f.Close();
         }
      }

      private void btnHelp_Clicked(object sender, EventArgs e) {
      // ---------------------------------------------------------------
         var addr = "http://www.zeemerixdata.com/NflData/Help/Help1.html";
         Device.OpenUri(new Uri(addr));

      }

   }
}
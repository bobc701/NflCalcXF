using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static NflCalcXF.Services.Repository;
using System.IO;

namespace NflCalcXF.Views
{
	public partial class HelpPage : ContentPage
	{
      public HelpPage() {
         InitializeComponent();
         this.Title = "Pro Football What-If Help";

         // Logic here to read help file, help.txt, and display.
         //using (StreamReader f = GetTextFileOnDisk("help")) {
         //   var htmlSource = new HtmlWebViewSource { Html = f.ReadToEnd() };
         //   htmlSource.BaseUrl = @"file:///android_asset/";
         //   WebView1.Source = htmlSource;
         //   WebView1.IsVisible = true;
         //   f.Close();
         //}

         //WebView1.Source = new HtmlWebViewSource() { BaseUrl = @"http://www.4bcx.com/NflData/Help/help1.html" };
         //WebView1.IsVisible = true;

         //Device.OpenUri(new Uri(@"http://www.4bcx.com/NflData/Help/help1.html"));
      }
	}
}
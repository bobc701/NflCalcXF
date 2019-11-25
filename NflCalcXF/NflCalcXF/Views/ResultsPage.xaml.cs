using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NflCalcXF.Views
{
	
	public partial class ResultsPage : ContentPage
	{
		public ResultsPage (string result, string resultType, string preface, string footer)
		{
			InitializeComponent ();
         this.Title = resultType;
         lblResults.Text = result;
         prefaceLabel.Text = preface;
         footerLabel.Text = footer;
		}
	}
}
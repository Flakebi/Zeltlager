using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager
{
	public partial class LogPage : ContentPage
	{
		public bool info{ get; set; }
		public bool warning{ get; set; }
		public bool error{ get; set; }
		public bool exception { get; set; }
		public LogPage()
		{
			InitializeComponent();
			info = false;
			warning = false;
			error = true;
			exception = true;
			BindingContext = this;
			updateUI(null, null);
		}

		void updateUI(object sender, EventArgs e)
		{
			logLabel.Text = LagerBase.Log.PrintLog(info, warning, error, exception);
		}

		async void OnDeleteClicked(object sender, EventArgs e)
		{
			await LagerBase.Log.ClearLog();
			updateUI(null, null);
		}
	}
}

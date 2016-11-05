using System;

using Xamarin.Forms;

namespace Zeltlager
{
	public partial class LogPage : ContentPage
	{
		public bool Info { get; set; }
		public bool Warning { get; set; }
		public bool Error { get; set; }
		public bool Exception { get; set; }
		public LogPage()
		{
			InitializeComponent();

			// Only display errors and exceptions by default
			Info = false;
			Warning = false;
			Error = true;
			Exception = true;

			BindingContext = this;
			UpdateUI(null, null);
		}

		void UpdateUI(object sender, EventArgs e)
		{
			logLabel.Text = LagerManager.Log.Print(Info, Warning, Error, Exception);
		}

		async void OnDeleteClicked(object sender, EventArgs e)
		{
			await LagerManager.Log.Clear();
			UpdateUI(null, null);
		}
	}
}

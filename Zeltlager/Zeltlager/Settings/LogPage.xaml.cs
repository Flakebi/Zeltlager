using System;

using Xamarin.Forms;
using Zeltlager.Client;

namespace Zeltlager
{
	public partial class LogPage : ContentPage
	{
		public LagerClient Lager { get; set; }

		public LogPage(LagerClient lager)
		{
			InitializeComponent();
			this.Lager = lager;
			BindingContext = lager.ClientManager.Settings;
			UpdateUI(null, null);
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void UpdateUI(object sender, EventArgs e)
		{
			logLabel.Text = LagerManager.Log.Print(Lager.ClientManager.Settings.ShowInfoInLog, 
			                                       Lager.ClientManager.Settings.ShowWarningInLog, 
			                                       Lager.ClientManager.Settings.ShowErrorInLog, 
			                                       Lager.ClientManager.Settings.ShowExceptionInLog);
		}

		async void OnDeleteClicked(object sender, EventArgs e)
		{
			await LagerManager.Log.Clear();
			UpdateUI(null, null);
		}
	}
}

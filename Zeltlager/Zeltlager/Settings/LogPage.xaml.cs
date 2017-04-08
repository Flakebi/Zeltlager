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

		protected override async void OnDisappearing()
		{
			base.OnDisappearing();
			try
			{
				await Lager.ClientManager.Settings.Save(Lager.ClientManager.IoProvider);
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("Settings", e);
				await DisplayAlert("Fehler!", "Das Speichern der Einstellungen schlug fehl.", "Ok");
			}
		}
	}
}

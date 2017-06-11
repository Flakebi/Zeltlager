using System;
using Xamarin.Forms;

namespace Zeltlager.Settings
{
	using Client;

	public partial class SettingsPage : ContentPage
	{
		LagerClient lager;

		public SettingsPage(LagerClient lager)
		{
			this.lager = lager;
			InitializeComponent();
			NavigationPage.SetBackButtonTitle(this, "");
			BindingContext = lager.ClientManager.Settings;
		}

		void OnLogClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new LogPage(lager));
		}

		async void OnTestDataClicked(object sender, EventArgs e)
		{
			await lager.CreateTestData();
			await DisplayAlert("Testdaten", "Es wurden Testdaten eingefügt!", "Ok");
		}

		void OnPacketsClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new InspectPackagesPage(lager));
		}

		void OnManageLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ManageLagerPage(lager, lager.ClientManager));
		}

		void OnCreditsClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new CreditsPage());
		}

		protected override async void OnDisappearing()
		{
			base.OnDisappearing();
			try
			{
				await lager.ClientManager.Settings.Save(lager.ClientManager.IoProvider);
			}
			catch (Exception e)
			{
				await LagerManager.Log.Exception("Settings", e);
				await DisplayAlert("Fehler!", "Das Speichern der Einstellungen schlug fehl.", "Ok");
			}
		}
	}
}

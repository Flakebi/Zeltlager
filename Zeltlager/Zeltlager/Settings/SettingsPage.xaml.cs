using System;

using Xamarin.Forms;
using Zeltlager.Client;

namespace Zeltlager.Settings
{
	public partial class SettingsPage : ContentPage
	{
		LagerClient lager;

		public SettingsPage(LagerClient lager)
		{
			this.lager = lager;
			InitializeComponent();
			NavigationPage.SetBackButtonTitle(this, "");
			string serverAddress = lager.ClientManager.Settings.ServerAddress;
			if (!string.IsNullOrEmpty(serverAddress))
			{
				ServerEntry.Text = serverAddress;
			}
		}

		void OnLogClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new LogPage(lager));
		}

		void OnManageLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ManageLagerPage(lager));
		}

		protected override async void OnDisappearing()
		{
			base.OnDisappearing();
			var settings = lager.ClientManager.Settings;
			if (ServerEntry.Text != settings.ServerAddress)
			{
				// Update the server address
				settings.ServerAddress = ServerEntry.Text;
				await settings.Save(lager.ClientManager.IoProvider);
			}
		}
	}
}

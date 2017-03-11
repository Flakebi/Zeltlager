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
		}

		void OnLogClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new LogPage(lager));
		}

		void OnManageLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ManageLagerPage(lager, lager.ClientManager));
		}
	}
}

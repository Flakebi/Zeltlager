using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.Client;
using System.Linq;

namespace Zeltlager.Settings
{
	public partial class ManageLagerPage : ContentPage
	{
		LagerClient lager;
		LagerClientManager manager;

		public ManageLagerPage(LagerClient lager, LagerClientManager manager)
		{
			InitializeComponent();
			this.lager = lager;
			this.manager = manager;
			NavigationPage.SetBackButtonTitle(this, "");
			string serverAddress = manager.Settings.ServerAddress;
			if (!string.IsNullOrEmpty(serverAddress))
			{
				ServerEntry.Text = serverAddress;
			}
			UpdateUI();
		}

		void UpdateUI()
		{
			if (lager == null)
			{
				UploadLagerButton.IsVisible = false;
			}
			if (!manager.Lagers.Values.Where(lb => ((LagerClient)lb) != lager).Any())
			{
				ChangeLagerButton.IsVisible = false;
			}
		}

		void OnCreateLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new CreateLagerPage(), true);
		}

		void OnDownloadLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new DownloadLagerPage(lager, manager), true);
		}

		void OnChangeLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ChangeLagerPage(lager), true);
		}

		void OnUploadLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new UploadLagerPage(lager), true);
		}

		protected override async void OnDisappearing()
		{
			base.OnDisappearing();
			var settings = manager.Settings;
			if (ServerEntry.Text != settings.ServerAddress)
			{
				// Update the server address
				settings.ServerAddress = ServerEntry.Text;
				await settings.Save(manager.IoProvider);
			}
		}
	}
}

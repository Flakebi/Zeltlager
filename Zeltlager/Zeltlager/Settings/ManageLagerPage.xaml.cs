using System;
using System.Collections.Generic;

using System.Linq;
using Xamarin.Forms;

namespace Zeltlager.Settings
{
	using Client;

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
				DeleteLagerButton.IsVisible = false;
				passwordLayout.IsVisible = false;
			}
			else
			{
				passwordLabel.Text = lager.Password;
			}
			if (!manager.Lagers.Values.Where(lb => ((LagerClient)lb) != lager).Any())
			{
				ChangeLagerButton.IsVisible = false;
			}
			else if (lager == null)
			{
				ChangeLagerButton.IsVisible = true;
				ChangeLagerButton.Text = "Lager ausw?hlen";
			}
		}

		void OnCreateLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new CreateLagerPage(), true);
		}

		async void OnDeleteLagerClicked(object sender, EventArgs e)
		{
			if (await DisplayAlert("Lager löschen", "Bist du dir sicher, dass du das Lager löschen willst?", "Ja", "Doch nicht"))
			{
				await manager.DeleteLager(lager);
				while (Navigation.NavigationStack.Count > 1)
					Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
				lager = null;
				UpdateUI();
			}
		}

		void OnDownloadLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new DownloadLagerPage(lager, manager), true);
		}

		void OnChangeLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ChangeLagerPage(lager, manager), true);
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
				try
				{
					await settings.Save(manager.IoProvider);
				}
				catch (Exception e)
				{
					await LagerManager.Log.Exception("Settings", e);
					await DisplayAlert("Fehler!", "Das Speichern der Einstellungen schlug fehl.", "Ok");
				}
			}
		}
	}
}

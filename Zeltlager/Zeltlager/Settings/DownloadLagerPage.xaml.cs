using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.Client;
using System.Linq;

namespace Zeltlager.Settings
{
	public partial class DownloadLagerPage : ContentPage
	{
		
		LagerClient lager;
		LagerClientManager manager;
		// the lager data currently downloaded from sever & server lager id
		Dictionary<int, LagerData> lagerDataList;
		string password;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Zeltlager.Settings.DownloadLagerPage"/> class.
		/// </summary>
		/// <param name="lager">The current lager, null if starting</param>
		/// <param name="manager">The manager can not be null.</param>
		public DownloadLagerPage(LagerClient lager, LagerClientManager manager)
		{
			InitializeComponent();
			NavigationPage.SetBackButtonTitle(this, "");
			this.lager = lager;
			this.manager = manager;
		}

		async void OnRequestClicked(object sender, EventArgs e)
		{
			password = PasswordEntry.Text ?? "";

			LoadingScreen ls = new LoadingScreen();
			await Navigation.PushModalAsync(new NavigationPage(ls), false);

			if (string.IsNullOrEmpty(manager.Settings.ServerAddress))
			{
				await DisplayAlert("Achtung!", "Bitte eine Serveradresse angeben.", "Ok");
			}
			else 
			{
				try
				{
					if (lagerDataList == null)
					{
						lagerDataList = await manager.RemoteListLagers(status => ls.Status = status.GetMessage());
					}
					List<LagerData> decryptedLagers = new List<LagerData>();
					foreach (var d in lagerDataList)
					{
						if (await d.Value.Decrypt(password))
						{
							ls.Status = "Lager " + d.Key + " erfolgreich entschlüsselt";
							decryptedLagers.Add(d.Value);
						}
					}

					if (vsl.Children.Last() is SearchableListView<LagerData>)
						vsl.Children.RemoveAt(vsl.Children.Count - 1);
					vsl.Children.Add(new SearchableListView<LagerData>(decryptedLagers, null, null, OnLagerClicked));
				}
				catch (Exception ex)
				{
					await LagerManager.Log.Exception("Request Lagers from Password", ex);
					await DisplayAlert("Fehler", "Das Anfragen der Lager vom Server ist fehlgeschlagen", "Ok");
				}
			}

			await Navigation.PopModalAsync(false);
		}

		async void OnLagerClicked(LagerData lagerData)
		{
			LoadingScreen ls = new LoadingScreen();
			await Navigation.PushModalAsync(new NavigationPage(ls), false);

			int serverId = lagerDataList.First(kv => kv.Value == lagerData).Key;
			LagerClient newLager = (LagerClient)manager.Lagers.FirstOrDefault(kv => kv.Value.Data.Data.SequenceEqual(lagerData.Data)).Value;
			try
			{
				if (newLager == null)
				{
					newLager = await manager.DownloadLager(serverId, lagerData, password, status => ls.Status = status.GetMessage(), status => ls.Status = status.GetMessage());
				}
				// Change the lager displayed in the app
				((App)Application.Current).ChangeLager(lager, newLager);
			}
			catch (Exception ex)
			{
				await LagerManager.Log.Exception("Change Lager after downloading", ex);
				await DisplayAlert("Fehler", "Das Wechseln des Lagers nach dem Herunterladen ist fehlgeschlagen", "OK");
			}

			await Navigation.PopModalAsync(false);
		}
	}
}

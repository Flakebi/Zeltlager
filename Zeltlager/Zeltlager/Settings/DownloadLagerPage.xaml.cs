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
		// the lager data currently downloaded from sever & server lager id
		Dictionary<int, LagerData> lagerDataList;
		string password;

		public DownloadLagerPage(LagerClient lager)
		{
			InitializeComponent();
			NavigationPage.SetBackButtonTitle(this, "");
			this.lager = lager;
		}

		async void OnRequestClicked(object sender, EventArgs e)
		{
			password = PasswordEntry.Text ?? "";

			LoadingScreen ls = new LoadingScreen();
			await Navigation.PushModalAsync(new NavigationPage(ls), false);

			if (lagerDataList == null)
			{
				lagerDataList = await lager.ClientManager.RemoteListLagers(status => ls.Status = status.GetMessage());
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

			await Navigation.PopModalAsync(false);
		}

		async void OnLagerClicked(LagerData lagerData)
		{
			LoadingScreen ls = new LoadingScreen();
			await Navigation.PushModalAsync(new NavigationPage(ls), false);

			int serverId = lagerDataList.First(kv => kv.Value == lagerData).Key;
			LagerClient newLager = (LagerClient)lager.ClientManager.Lagers.FirstOrDefault(kv => kv.Value.Data.Data.SequenceEqual(lagerData.Data)).Value;
			if (newLager == null)
			{
				newLager = await lager.ClientManager.DownloadLager(serverId, lagerData, password, status => ls.Status = status.GetMessage(), status => ls.Status = status.GetMessage());
			}
			// Change the lager displayed in the app
			((App)Application.Current).ChangeLager(lager, newLager);

			await Navigation.PopModalAsync(false);
		}
	}
}

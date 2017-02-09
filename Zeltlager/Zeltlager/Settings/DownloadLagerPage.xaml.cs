using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.Client;
using System.Linq;

namespace Zeltlager.Settings
{
	public partial class DownloadLagerPage : ContentPage
	{
		LagerClientManager manager;
		// the lager data currently downloaded from sever & server lager id
		Dictionary<int, LagerData> lagerDataList;
		string password;

		public DownloadLagerPage(LagerClientManager manager)
		{
			InitializeComponent();
			NavigationPage.SetBackButtonTitle(this, "");
			this.manager = manager;
		}

		async void OnRequestClicked(object sender, EventArgs e)
		{
			password = PasswordEntry.Text;
			// TODO request matching lagers from Server
			//IReadOnlyList<Lager> lagers = 
			//vsl.Children.Add(new SearchableListView)
			LoadingScreen ls = new LoadingScreen();
			await Navigation.PushModalAsync(new NavigationPage(ls), false);
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
			vsl.Children.Add(new SearchableListView<LagerData>(decryptedLagers, null, null, OnLagerClicked));
			await Navigation.PopModalAsync(false);
		}

		async void OnLagerClicked(LagerData lagerData)
		{
			LoadingScreen ls = new LoadingScreen();
			await Navigation.PushModalAsync(new NavigationPage(ls), false);
			int serverId = lagerDataList.First(kv => kv.Value == lagerData).Key;
			LagerClient lager = await manager.DownloadLager(serverId, lagerData, password, status => ls.Status = status.GetMessage(), status => ls.Status = status.GetMessage());
			await Navigation.PopModalAsync(false);
		}
	}
}

﻿using Xamarin.Forms;
using System.Threading.Tasks;
using Zeltlager.Client;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Zeltlager.Settings
{
	public class InspectPackagesPage : ContentPage
	{
		LagerClient lager;

		public InspectPackagesPage(LagerClient lager)
		{
			this.lager = lager;
			NavigationPage.SetBackButtonTitle(this, "");

			Task<List<DataPacket>> packetsTask = lager.GetHistory();
			packetsTask.Wait();
			Content = new SearchableListView<DataPacket>(packetsTask.Result.Take(100).ToList(), null, null, OnErwischtGameClicked);

			Title = "Datenpakete";
			Style = (Style)Application.Current.Resources["BaseStyle"];
			Padding = new Thickness(10);
		}

		Task OnDeleteClicked(DataPacket packet) => Task.WhenAll();

		void OnEditClicked(DataPacket packet) { }

		async void OnErwischtGameClicked(DataPacket packet)
		{
			bool revert = await DisplayAlert("Paket reverten?", "Möchten sie das Paket '" + packet + "' reverten?", "Ja", "Nein");
			if (revert)
			{
				try
				{
					await lager.AddPacket(new RevertPacket(lager.ClientSerialiser, new LagerClientSerialisationContext(lager), packet.Id));
				} catch (LagerException e)
				{
					await DisplayAlert("Achtung!", "Beim Anwenden der Datenpakete ist ein Fehler aufgetreten. " +
					                   "Du möchtest ihr neu erstelltes Revert-Package eventuell wieder reverten.\n" + e.Message, "Ok");
				}
			}
		}
	}
}



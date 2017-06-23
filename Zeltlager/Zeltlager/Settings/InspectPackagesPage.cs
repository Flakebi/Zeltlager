using Xamarin.Forms;
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

			SetContent();

			Title = "Datenpakete";
			Style = (Style)Application.Current.Resources["BaseStyle"];
			Padding = new Thickness(10);
		}

		async Task SetContent()
		{
			List<DataPacket> packetsList = await lager.GetHistory();
			Content = new SearchableListView<DataPacket>(packetsList.OrderByDescending(p => p.Timestamp).Take(100).ToList(), null, null, OnDataPacketClicked);
		}

		Task OnDeleteClicked(DataPacket packet) => Task.WhenAll();

		void OnEditClicked(DataPacket packet) { }

		async void OnDataPacketClicked(DataPacket packet)
		{
			bool revert = await DisplayAlert("Paket rückgängig machen?", "Möchten sie das Paket '" + packet + "' rückgängig machen?", "Ja", "Lieber nicht");
			if (revert)
			{
				try
				{
					await lager.AddPacket(new RevertPacket(lager.ClientSerialiser, new LagerClientSerialisationContext(lager), packet.Id));
				} catch (LagerException e)
				{
					await DisplayAlert("Achtung!", "Beim Anwenden der Datenpakete ist ein Fehler aufgetreten. " +
					                   "Du möchtest dein neu erstelltes Revert-Packet eventuell wieder rückgangig machen.\n" + e.Message, "Ok");
				}
			}
		}
	}
}



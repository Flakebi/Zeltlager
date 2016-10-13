using System;
using Zeltlager.UAM;

using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;
using Zeltlager.DataPackets;

namespace Zeltlager.General
{
	using Client;

	public partial class TentsPage : ContentPage
	{
		public TentsPage()
		{
			InitializeComponent();
			Content = new SearchableListView<Tent>(LagerClient.CurrentLager.Tents, OnContextActionEdit, OnContextActionDelete);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			var tentNumber = (byte) 0;
			if (LagerClient.CurrentLager.Tents.Any())
				tentNumber = (byte) (LagerClient.CurrentLager.Tents.Max(t => t.Number) + 1);
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Tent>(new Tent(new TentId(),tentNumber, "", new List<Member>()),true)));
		}

		void OnContextActionEdit(object sender)
		{
			DisplayAlert("ContextAction called", "on Edit in TentsPage", "ok");
			Tent t = (Tent)((MenuItem)sender).CommandParameter;
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Tent>(t, false)), true);
		}

		async void OnContextActionDelete(object sender)
		{
			await DisplayAlert("ContextAction called", "on Delete in TentsPage", "ok");
			await LagerClient.CurrentLager.AddPacket(new DeleteTent((Tent)((MenuItem)sender).CommandParameter));
		}
	}
}


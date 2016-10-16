using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace Zeltlager.General
{
	using Client;
	using DataPackets;
	using UAM;

	public partial class TentsPage : ContentPage
	{
		public TentsPage()
		{
			InitializeComponent();
			Content = new SearchableListView<Tent>(LagerClient.CurrentLager.Tents, OnContextActionEdit, OnContextActionDelete);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			var tentNumber = (byte)0;
			if (LagerClient.CurrentLager.Tents.Any())
				tentNumber = (byte)(LagerClient.CurrentLager.Tents.Max(t => t.Number) + 1);
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Tent>(new Tent(new TentId(), tentNumber, "", true, new List<Member>()), true)));
		}

		void OnContextActionEdit(Tent tent)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Tent>(tent, false)), true);
		}

		async void OnContextActionDelete(Tent tent)
		{
			await LagerClient.CurrentLager.AddPacket(new DeleteTent(tent));
		}
	}
}


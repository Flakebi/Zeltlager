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
		LagerClient lager;

		public TentsPage(LagerClient lager)
		{
			InitializeComponent();
			this.lager = lager;
			Content = new SearchableListView<Tent>(lager.Tents, OnContextActionEdit, OnContextActionDelete, OnTentClick);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			var tentNumber = (byte)0;
			if (lager.Tents.Any())
				tentNumber = (byte)(lager.Tents.Max(t => t.Number) + 1);
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Tent>(new Tent(new TentId(), tentNumber, "", true, new List<Member>()), true, lager)), true);
		}

		void OnContextActionEdit(Tent tent)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Tent>(tent, false, lager)), true);
		}

		async void OnContextActionDelete(Tent tent)
		{
			await lager.AddPacket(new DeleteTent(tent));
		}

		void OnTentClick(Tent tent)
		{
			Navigation.PushAsync(new TentDetailPage(tent, lager));
		}
	}
}


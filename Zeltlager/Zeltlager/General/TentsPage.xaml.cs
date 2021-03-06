using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;
using System.Threading.Tasks;

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
			Padding = new Thickness(10);
			this.lager = lager;
			Content = new SearchableListView<Tent>(lager.VisibleTents, OnEditClicked, OnDeleteClicked, OnTentClick);
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			int tentNumber = 0;
			if (lager.Tents.Any())
				tentNumber = lager.Tents.Max(t => t.Number) + 1;
			Navigation.PushAsync(new UniversalAddModifyPage<Tent, Tent>(
				new Tent(null, tentNumber, "", true, new List<Member>(), lager), true, lager));
		}

		void OnEditClicked(Tent tent)
		{
			Navigation.PushAsync(new UniversalAddModifyPage<Tent, Tent>(tent, false, lager));
		}

		async Task OnDeleteClicked(Tent tent)
		{
			await tent.Delete(lager);
			OnAppearing();
		}

		public void OnTentClick(Tent tent)
		{
			Navigation.PushAsync(new TentDetailPage(tent, lager));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Content = new SearchableListView<Tent>(lager.VisibleTents, OnEditClicked, OnDeleteClicked, OnTentClick);
		}
	}
}


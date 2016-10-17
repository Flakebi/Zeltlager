using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.UAM;
using Zeltlager.Client;

namespace Zeltlager.Competition
{
	public partial class CompetitionHandlerPage : ContentPage
	{
		LagerClient lager;
		public CompetitionHandlerPage(LagerClient lager)
		{
			InitializeComponent();
			this.lager = lager;
			Content = new SearchableListView<Competition>(lager.CompetitionHandler.Competitions, OnEdit, OnDelete);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new UniversalAddModifyPage<Competition>(new Competition(lager, ""), true, lager),true);
		}

		void OnEdit(object sender)
		{
			// TODO edit competitons
		}

		void OnDelete(object sender)
		{
			// TODO delete competetions, packets
		}
	}
}

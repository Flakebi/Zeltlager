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
			Content = new SearchableListView<Competition>(lager.CompetitionHandler.Competitions, OnEdit, OnDelete, OnClick);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new UniversalAddModifyPage<Competition>(new Competition(null, "", lager), true, lager),true);
		}

		void OnEdit(Competition comp)
		{
			// TODO edit competitons
		}

		void OnDelete(Competition comp)
		{
			// TODO delete competetions, packets
		}

		void OnClick(Competition comp)
		{
			// TODO what should happen if competition is selected
		}
	}
}

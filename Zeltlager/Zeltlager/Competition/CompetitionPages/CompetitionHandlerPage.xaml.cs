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
			Content = new SearchableListView<Competition>(lager.CompetitionHandler.Competitions, OnEditClicked, OnDeleteClicked, OnCompetitionClick);
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnAddClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Competition, Rankable>(new Competition(null, "", lager), true, lager)),true);
		}

		void OnEditClicked(Competition comp)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Competition, Rankable>(comp,false, lager)));
		}

		void OnDeleteClicked(Competition comp)
		{
			comp.IsVisible = false;
		}

		void OnCompetitionClick(Competition comp)
		{
			Navigation.PushAsync(new CompetitionPage(comp, lager));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Content = new SearchableListView<Competition>(lager.CompetitionHandler.Competitions, OnEditClicked, OnDeleteClicked, OnCompetitionClick);
		}
	}
}

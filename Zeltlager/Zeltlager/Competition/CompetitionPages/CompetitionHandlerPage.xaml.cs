using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.UAM;
using Zeltlager.Client;

namespace Zeltlager.Competition
{
	public partial class CompetitionHandlerPage : ContentPage
	{
		public CompetitionHandlerPage()
		{
			InitializeComponent();
			Content = new SearchableListView<Competition>(LagerClient.CurrentLager.CompetitionHandler.Competitions, OnEdit, OnDelete);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new NavigationPage(new UniversalAddModifyPage<Competition>(new Competition(LagerClient.CurrentLager, ""), true)),true);
		}

		void OnEdit(object sender)
		{
			
		}

		void OnDelete(object sender)
		{
			
		}
	}
}

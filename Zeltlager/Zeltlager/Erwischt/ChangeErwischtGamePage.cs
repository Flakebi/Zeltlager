﻿using System.Threading.Tasks;
using Xamarin.Forms;

namespace Zeltlager.Erwischt
{
	using Client;

	public class ChangeErwischtGamePage : ContentPage
	{
		LagerClient lager;

		public ChangeErwischtGamePage(LagerClient lager)
		{
			this.lager = lager;
			NavigationPage.SetBackButtonTitle(this, "");
			Content = new SearchableListView<ErwischtGame>(lager.ErwischtHandler.VisibleGames,
													   OnEditClicked, OnDeleteClicked, OnErwischtGameClicked);
			Title = "Spiel wechseln";
			Style = (Style)Application.Current.Resources["BaseStyle"];
			Padding = new Thickness(10);
		}

		Task OnDeleteClicked(ErwischtGame game) => Task.WhenAll();

		void OnEditClicked(ErwischtGame game) { }

		void OnErwischtGameClicked(ErwischtGame game)
		{

		}
	}
}
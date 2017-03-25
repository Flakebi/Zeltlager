using System;

using Xamarin.Forms;
using System.Threading.Tasks;
using Zeltlager.Client;

namespace Zeltlager.Erwischt
{
	public class ChangeErwischtGamePage : ContentPage
	{
		LagerClient lager;

		public ChangeErwischtGamePage(LagerClient lager)
		{
			this.lager = lager;
			Content = new SearchableListView<ErwischtGame>(lager.ErwischtHandler.VisibleGames,
			                                           OnEditClicked, OnDeleteClicked, OnErwischtGameClicked);
			Title = "Spiel wechseln";
			Style = (Style)Application.Current.Resources["BaseStyle"];
			Padding = new Thickness(10);
			NavigationPage.SetBackButtonTitle(this, "");
		}

		async Task OnDeleteClicked(ErwischtGame game)
		{
			game.IsVisible = false;
			// TODO packages
		}

		void OnEditClicked(ErwischtGame game) { }

		void OnErwischtGameClicked(ErwischtGame game)
		{
			lager.ErwischtHandler.CurrentGame = game;
			Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);
			Navigation.InsertPageBefore(new ErwischtPage(game, lager), this);
			Navigation.PopAsync(true);
		}
	}
}


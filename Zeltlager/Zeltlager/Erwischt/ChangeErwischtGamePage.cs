using System;

using Xamarin.Forms;
using System.Threading.Tasks;

namespace Zeltlager.Erwischt
{
	public class ChangeErwischtGamePage : ContentPage
	{
		ErwischtHandler handler;

		public ChangeErwischtGamePage(ErwischtHandler handler)
		{
			this.handler = handler;
			Content = new SearchableListView<Erwischt>(handler.VisibleGames,
			                                           OnEditClicked, OnDeleteClicked, OnErwischtGameClicked);
			Title = "Spiel wechseln";
			Style = (Style)Application.Current.Resources["BaseStyle"];
			Padding = new Thickness(10);
			NavigationPage.SetBackButtonTitle(this, "");
		}

		async Task OnDeleteClicked(Erwischt game)
		{
			game.IsVisible = false;
			// TODO packages
		}

		void OnEditClicked(Erwischt game) { }

		void OnErwischtGameClicked(Erwischt game)
		{
			handler.CurrentGame = game;
			Navigation.PopAsync(true);
		}
	}
}


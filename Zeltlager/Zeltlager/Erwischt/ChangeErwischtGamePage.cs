using System.Threading.Tasks;
using Xamarin.Forms;

namespace Zeltlager.Erwischt
{
	using Client;
	using DataPackets;
	using Serialisation;

	public class ChangeErwischtGamePage : ContentPage
	{
		LagerClient lager;
		ErwischtGame currentGame;

		public ChangeErwischtGamePage(LagerClient lager, ErwischtGame currentGame)
		{
			this.lager = lager;
			this.currentGame = currentGame;
			NavigationPage.SetBackButtonTitle(this, "");
			Title = "Spiel wechseln";
			Style = (Style)Application.Current.Resources["BaseStyle"];
			Padding = new Thickness(10);
			UpdateUI();
		}

		void UpdateUI()
		{
			Content = new SearchableListView<ErwischtGame>(lager.ErwischtHandler.VisibleGames,
														  null, OnDeleteClicked, OnErwischtGameClicked);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}

		async Task OnDeleteClicked(ErwischtGame game)
		{
			game.IsVisible = false;
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(lager);
			Serialiser<LagerClientSerialisationContext> serialiser = lager.ClientSerialiser;
			await context.LagerClient.AddPacket(await DeleteErwischtPacket.Create(serialiser, context, game));

			if (lager.ErwischtHandler.VisibleGames.Count == 0)
			{
				// If there are no more games left, go to the main screen
				await Navigation.PopToRootAsync();
				return;
			}
			else if (game == currentGame)
			{
				// If the current game was deleted, select the newest game but stay in this screen
				Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
				Navigation.InsertPageBefore(new ErwischtPage(lager.ErwischtHandler.GetNewestGame(), lager), this);
			}
			OnAppearing();
		}

		void OnErwischtGameClicked(ErwischtGame game)
		{
			Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
			Navigation.InsertPageBefore(new ErwischtPage(game, lager), this);
			Navigation.PopAsync();
		}
	}
}

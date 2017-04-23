using System;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Zeltlager.Erwischt
{
	using Client;
	using UAM;

	public partial class ErwischtPage : ContentPage
	{
		ErwischtGame game;
		LagerClient lager;

		public ErwischtPage(ErwischtGame game, LagerClient lager)
		{
			InitializeComponent();
			this.game = game;
			this.lager = lager;
			Title = game.Name;
			NavigationPage.SetBackButtonTitle(this, "");
			UpdateUI();
		}

		void UpdateUI()
		{
			if (lager.ClientManager.Settings.HideDeadParticipants)
			{
				Content = new SearchableListView<ErwischtParticipant>(
					game.ErwischtParticipants.Where(ep => ep.IsAlive).ToList(),
					null, null, OnErwischtParticipantClicked);
			}
			else
			{
				Content = new SearchableListView<ErwischtParticipant>(
					game.ErwischtParticipants,
					null, null, OnErwischtParticipantClicked);
			}
		}

		void OnErwischtParticipantClicked(ErwischtParticipant member)
		{
			Navigation.PushAsync(new ErwischtParticipantDetailPage(member));
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new UniversalAddModifyPage<ErwischtGame, ErwischtGame>(new ErwischtGame("", lager), true, lager));
		}

		void OnChangeGameButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ChangeErwischtGamePage(lager, game));
		}

		void OnStatisticsButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ErwischtStatisticsPage(lager, game));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}
	}
}

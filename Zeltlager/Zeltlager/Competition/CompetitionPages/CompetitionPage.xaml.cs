using System;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Zeltlager.Competition
{
	using Client;
	using UAM;

	public partial class CompetitionPage : TabbedPage
	{
		Competition competition;
		LagerClient lager;
		RankingView rankingView;

		public CompetitionPage(Competition competition, LagerClient lager)
		{
			InitializeComponent();
			this.competition = competition;
			this.lager = lager;

			BindingContext = competition;
			NavigationPage.SetBackButtonTitle(this, "");
			rankingView = new RankingView(lager, competition, true, OnEditClickedParticipant, OnDeleteClickedParticipant);
			UpdateUI();
		}

		void UpdateUI()
		{
			var stationList = new SearchableListView<Station>(
				competition.Stations.Where(s => s.IsVisible).ToList(),
				OnEditClickedStation, OnDeleteClickedStation, OnStationClicked);

			competition.UpdateRanking();
			rankingView.UpdateUI();

			stationPage.Content = stationList;
			participantPage.Content = rankingView;
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Page addPage;
			if (CurrentPage == stationPage)
			{
				if (!lager.VisibleSupervisors.Any())
				{
					DisplayAlert("Achtung!", "Mindestens ein Betreuer erforderlich.", "Ok");
					return;
				}
				addPage = new UniversalAddModifyPage<Station, Rankable>(new Station(null, "", competition), true, lager);
			}
			else
				addPage = new AddEditParticipantPage(new GroupParticipant(null, "", competition), true);
			Navigation.PushAsync(addPage);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}

		void OnStationClicked(Station station)
		{
			Navigation.PushAsync(new StationPage(station));
		}

		void OnEditClickedStation(Station station)
		{
			Navigation.PushAsync(new UniversalAddModifyPage<Station, Rankable>(station, false, lager));
		}

		async Task OnDeleteClickedStation(Station station)
		{
			await station.Delete(lager);
			UpdateUI();
		}

		void OnEditClickedParticipant(CompetitionResult result)
		{
			Navigation.PushAsync(new AddEditParticipantPage(result.Participant, false));
		}

		async Task OnDeleteClickedParticipant(CompetitionResult result)
		{
			await result.Participant.Delete(lager);
			UpdateUI();
		}
	}
}

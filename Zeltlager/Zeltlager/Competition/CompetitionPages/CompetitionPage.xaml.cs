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

		public CompetitionPage(Competition competition, LagerClient lager)
		{
			InitializeComponent();
			this.competition = competition;
			this.lager = lager;

			BindingContext = competition;
			NavigationPage.SetBackButtonTitle(this, "");
			UpdateUI();
		}

		void UpdateUI()
		{
			var stationList = new SearchableListView<Station>(
				competition.Stations.Where(s => s.IsVisible).ToList(),
				OnEditClickedStation, OnDeleteClickedStation, OnStationClicked);

			var participantList = new SearchableListView<Participant>(
				competition.Participants.Where(p => p.IsVisible).ToList(),
				OnEditClickedParticipant, OnDeleteClickedParticipant, OnParticipantClicked);

			stationPage.Content = stationList;
			participantPage.Content = participantList;
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Page addPage;
			if (CurrentPage == stationPage)
				addPage = new UniversalAddModifyPage<Station, Rankable>(new Station(null, "", competition), true, lager);
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
			OnAppearing();
		}

		void OnParticipantClicked(Participant participant)
		{
			// TODO Packet Detail Page?
		}

		void OnEditClickedParticipant(Participant participant)
		{
			Navigation.PushAsync(new AddEditParticipantPage(participant, false));
		}

		async Task OnDeleteClickedParticipant(Participant participant)
		{
			await participant.Delete(lager);
			OnAppearing();
		}
	}
}

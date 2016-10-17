using System;
using Xamarin.Forms;
using Zeltlager.UAM;
using Zeltlager.Client;

namespace Zeltlager.Competition
{
	public partial class CompetitionPage : ContentPage
	{
		Competition competition;
		LagerClient lager;

		public CompetitionPage(Competition competition, LagerClient lager)
		{
			InitializeComponent();
			this.competition = competition;
			this.lager = lager;

			StackLayout vsl = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			StackLayout stationHeader = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};
			stationHeader.Children.Add(new Label { Text = "Stationen" });
			Button addStation = new Button { Text = Icons.ADD };
			addStation.Clicked += OnAddStationClicked;
			stationHeader.Children.Add(addStation);

			var stationList = new SearchableListView<Station>(competition.Stations, OnEditStation, OnDeleteStation);

			StackLayout participantHeader = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};
			participantHeader.Children.Add(new Label { Text = "Stationen" });
			Button addParticipant = new Button { Text = Icons.ADD };
			addParticipant.Clicked += OnAddParticipantClicked;
			participantHeader.Children.Add(addParticipant);

			var participantList = new SearchableListView<Participant>(competition.Participants, OnEditParticipant, OnDeleteParticipant);

			vsl.Children.Add(stationHeader);
			vsl.Children.Add(stationList);
			vsl.Children.Add(participantHeader);
			vsl.Children.Add(participantList);
		}

		void OnStationSelected(object sender, SelectedItemChangedEventArgs e)
		{
			//ItemSelected is called on deselection, which results in SelectedItem being set to null
			if (e.SelectedItem == null)
				return;
			Navigation.PushAsync(new NavigationPage(new StationPage()));
		}

		void OnAddStationClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new NavigationPage(new UniversalAddModifyPage<Station>(new Station("", competition), true, lager)), true);
		}

		void OnAddParticipantClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new NavigationPage(new UniversalAddModifyPage<Participant>(new Participant("", competition), true, lager)), true);
		}

		#region Searchable implementaition
		void OnEditStation(Station station)


		{
			Navigation.PushAsync(new NavigationPage(new UniversalAddModifyPage<Station>(station, false, lager)), true);
		}

		void OnDeleteStation(Station station)
		{
			// TODO packets
		}

		void OnEditParticipant(Participant participant)
		{
			Navigation.PushAsync(new NavigationPage(new UniversalAddModifyPage<Participant>(participant, false, lager)), true);
		}

		void OnDeleteParticipant(Participant participant)
		{
			// TODO packets
		}
		#endregion
	}
}

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

			CreateUI();
		}

		void CreateUI()
		{
			StackLayout vsl = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Padding = new Thickness(10, 2)
			};

			StackLayout stationHeader = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};
			stationHeader.Children.Add(new Label
			{
				Text = "Stationen",
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.Center
			});
			Button addStation = new Button { Image = Icons.ADD, HorizontalOptions = LayoutOptions.End };
			addStation.Clicked += OnAddStationClicked;
			stationHeader.Children.Add(addStation);

			var stationList = new SearchableListView<Station>(competition.Stations, OnEditStation, OnDeleteStation, OnClickStation);

			StackLayout participantHeader = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};
			participantHeader.Children.Add(new Label
			{
				Text = "Teilnehmer",
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.Center
			});
			Button addParticipant = new Button { Image = Icons.ADD, HorizontalOptions = LayoutOptions.End };
			addParticipant.Clicked += OnAddParticipantClicked;
			participantHeader.Children.Add(addParticipant);

			var participantList = new SearchableListView<Participant>(competition.Participants, OnEditParticipant, OnDeleteParticipant, OnClickParticipant);

			vsl.Children.Add(stationHeader);
			vsl.Children.Add(stationList);
			vsl.Children.Add(participantHeader);
			vsl.Children.Add(participantList);

			Content = vsl;
		}

		//void OnStationSelected(object sender, SelectedItemChangedEventArgs e)
		//{
		//	//ItemSelected is called on deselection, which results in SelectedItem being set to null
		//	if (e.SelectedItem == null)
		//		return;
		//	Navigation.PushAsync(new NavigationPage(new StationPage()));
		//}

		void OnAddStationClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Station>(new Station(null, "", competition), true, lager)), true);
		}

		void OnAddParticipantClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new AddEditParticipantPage(new GroupParticipant(null, "", competition), true)), true);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			CreateUI();
		}

		#region Searchable implementaition

		void OnClickStation(Station station)
		{
			Navigation.PushAsync(new StationPage(station));
		}

		void OnEditStation(Station station)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Station>(station, false, lager)), true);
		}

		void OnDeleteStation(Station station)
		{
			// TODO packets
		}

		void OnClickParticipant(Participant participant)
		{
			// TODO Packet Detail Page?
		}

		void OnEditParticipant(Participant participant)
		{
			Navigation.PushModalAsync(new NavigationPage(new AddEditParticipantPage(participant, false)), true);
		}

		void OnDeleteParticipant(Participant participant)
		{
			// TODO packets
		}
		#endregion
	}
}

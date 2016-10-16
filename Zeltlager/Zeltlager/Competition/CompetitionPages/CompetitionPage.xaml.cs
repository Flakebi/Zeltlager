using System;
using Xamarin.Forms;

namespace Zeltlager.Competition
{
	public partial class CompetitionPage : ContentPage
	{
		Competition competition;

		public CompetitionPage(Competition competition)
		{
			InitializeComponent();
			this.competition = competition;

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


		}

		void OnAddStationClicked(object sender, EventArgs e)
		{
			
		}

		void OnAddParticipantClicked(object sender, EventArgs e)
		{
			
		}

		#region Searchable implementaition
		void OnEditStation(Station station)
		{
			
		}

		void OnDeleteStation(Station station)
		{
			
		}

		void OnEditParticipant(Participant participant)
		{
			
		}

		void OnDeleteParticipant(Participant participant)
		{
			
		}
		#endregion
	}
}

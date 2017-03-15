using System;
using Xamarin.Forms;
using Zeltlager.UAM;
using Zeltlager.Client;
using System.Linq;
using System.Threading.Tasks;

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

			NavigationPage.SetBackButtonTitle(this, "");
			UpdateUI();
		}

		void UpdateUI()
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

			var stationList = new SearchableListView<Station>(competition.Stations.Where(s => s.IsVisible).ToList(),
			                                                  OnEditClickedStation, OnDeleteClickedStation, OnStationClicked);

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

			var participantList = new SearchableListView<Participant>(competition.Participants.Where(p => p.IsVisible).ToList(),
			                                                          OnEditClickedParticipant, OnDeleteClickedParticipant, OnParticipantClicked);

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
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Station, Rankable>(new Station(null, "", competition), true, lager)), true);
		}

		void OnAddParticipantClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new AddEditParticipantPage(new GroupParticipant(null, "", competition), true)), true);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}

		#region Searchable implementaition

		void OnStationClicked(Station station)
		{
			Navigation.PushAsync(new StationPage(station));
		}

		void OnEditClickedStation(Station station)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Station, Rankable>(station, false, lager)), true);
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
			Navigation.PushModalAsync(new NavigationPage(new AddEditParticipantPage(participant, false)), true);
		}

		async Task OnDeleteClickedParticipant(Participant participant)
		{
			await participant.Delete(lager);
			OnAppearing();
		}
		#endregion
	}
}

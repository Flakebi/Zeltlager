using System;
using Xamarin.Forms;

namespace Zeltlager.Competition
{
	using UAM;

	public partial class StationPage : ContentPage
	{
		Station station;

		EventHandler updateWidth;

		public StationPage(Station station)
		{
			InitializeComponent();
			this.station = station;

			// One time UI setup
			BindingContext = station;

			participantResults.ItemTemplate = new DataTemplate(typeof(ParticipantResultCell));

			updateWidth = (sender, e) =>
			{
				increasingButton.WidthRequest = Width * 0.45;
				decreasingButton.WidthRequest = Width * 0.45;
			};

			updateWidth(null, null);
			SizeChanged += updateWidth;

			UpdateUI();
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnParticipantSelected(object sender, EventArgs e)
		{
			CompetitionResult item = (CompetitionResult)participantResults.SelectedItem;
			if (item != null)
				Navigation.PushAsync(new UniversalAddModifyPage<CompetitionResult, CompetitionResult>(
					item, false, station.GetLagerClient()));
			participantResults.SelectedItem = null;
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new UniversalAddModifyPage<CompetitionResult, CompetitionResult>(
				new CompetitionResult(null, station, null), true, station.GetLagerClient()));
		}

		void OnIncreasingButtonClicked(object sender, EventArgs e)
		{
			station.Ranking.Rank(true);
			UpdateUI();
		}

		void OnDecreasingButtonClicked(object sender, EventArgs e)
		{
			station.Ranking.Rank(false);
			UpdateUI();
		}

		void UpdateUI()
		{
			participantResults.BindingContext = station.Ranking.Results;
			// Set it to null first to refresh the list
			participantResults.ItemsSource = null;
			participantResults.ItemsSource = station.Ranking.Results;

			updateWidth(null, null);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}
	}
}

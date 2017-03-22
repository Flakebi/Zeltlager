using System;
using Xamarin.Forms;

namespace Zeltlager.Competition
{
	using UAM;

	public partial class StationPage : ContentPage
	{
		Station station;

		public StationPage(Station station)
		{
			InitializeComponent();
			this.station = station;

			CreateUI();
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<CompetitionResult, CompetitionResult>(new CompetitionResult(null, station, null), true, station.GetLagerClient())), true);
		}

		void OnIncreasingButtonClicked(object sender, EventArgs e)
		{
			station.Ranking.Rank(true);
			CreateUI();
		}

		void OnDecreasingButtonClicked(object sender, EventArgs e)
		{
			station.Ranking.Rank(false);
			CreateUI();
		}

		void CreateUI()
		{
			participantResults.ItemTemplate = new DataTemplate(typeof(ParticipantResultCell));
			participantResults.BindingContext = station.Ranking.Results;
			// Set it to null first to refresh the list
			participantResults.ItemsSource = null;
			participantResults.ItemsSource = station.Ranking.Results;
			participantResults.ItemSelected += (sender, e) =>
			{
				CompetitionResult item = (CompetitionResult)participantResults.SelectedItem;
				if (item != null)
					Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<CompetitionResult, CompetitionResult>(
						item, false, station.GetLagerClient())), true);
				participantResults.SelectedItem = null;
			};

			EventHandler updateWidth = (sender, e) =>
			{
				increasingButton.WidthRequest = Width * 0.45;
				decreasingButton.WidthRequest = Width * 0.45;
			};

			updateWidth(null, null);
			SizeChanged += updateWidth;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			CreateUI();
		}
	}
}

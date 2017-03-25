using System;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Zeltlager.Competition
{
	using Client;
	using UAM;

	public partial class RankingView : ContentView
	{
		LagerClient lager;
		Ranking ranking;

		EventHandler updateWidth;

		public RankingView(LagerClient lager, Ranking ranking, bool editable = false)
		{
			InitializeComponent();
			this.lager = lager;
			this.ranking = ranking;

			participantResults.ItemTemplate = new DataTemplate(typeof(ParticipantResultCell));

			updateWidth = (sender, e) =>
			{
				increasingButton.WidthRequest = Width * 0.45;
				decreasingButton.WidthRequest = Width * 0.45;
			};

			updateWidth(null, null);
			SizeChanged += updateWidth;

			UpdateUI();
		}

		public void UpdateUI()
		{
			// Set it to null first to refresh the list
			participantResults.ItemsSource = null;
			participantResults.ItemsSource = ranking.Results;

			updateWidth(null, null);
		}

		void UpdateHeaderSize()
		{
			// TODO Update the size of the images
		}

		void OnParticipantSelected(object sender, EventArgs e)
		{
			CompetitionResult item = (CompetitionResult)participantResults.SelectedItem;
			if (item != null)
				Navigation.PushAsync(new UniversalAddModifyPage<CompetitionResult, CompetitionResult>(
					item, false, lager));
			participantResults.SelectedItem = null;
		}

		void OnEditClickedParticipant(Participant participant)
		{
			Navigation.PushAsync(new NavigationPage(new AddEditParticipantPage(participant, false)), true);
		}

		async Task OnDeleteClickedParticipant(Participant participant)
		{
			await participant.Delete(lager);
			UpdateUI();
		}

		void OnIncreasingButtonClicked(object sender, EventArgs e)
		{
			ranking.Rank(true);
			UpdateUI();
		}

		void OnDecreasingButtonClicked(object sender, EventArgs e)
		{
			ranking.Rank(false);
			UpdateUI();
		}
	}
}

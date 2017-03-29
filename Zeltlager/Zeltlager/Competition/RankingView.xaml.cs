using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Zeltlager.Competition
{
	using Client;
	using UAM;

	public partial class RankingView : ContentView
	{
		LagerClient lager;
		Rankable rankable;
		Ranking ranking;

		EventHandler updateWidth;

		/// <summary>
		/// The ranking results including all not-ranked participants.
		/// </summary>
		List<CompetitionResult> totalRanking = new List<CompetitionResult>();

		public RankingView(LagerClient lager, Rankable rankable, Ranking ranking, bool editable = false)
		{
			InitializeComponent();
			this.lager = lager;
			this.rankable = rankable;
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
			// Update the total ranking
			totalRanking.Clear();
			totalRanking.AddRange(ranking.Results);
			totalRanking.AddRange(rankable.GetParticipants()
				.Except(ranking.Results.Select(r => r.Participant))
				.Select(p => new CompetitionResult(null, rankable, p)));

			// Set it to null first to refresh the list
			participantResults.ItemsSource = null;
			participantResults.ItemsSource = totalRanking;
		}

		void OnParticipantSelected(object sender, EventArgs e)
		{
			CompetitionResult item = (CompetitionResult)participantResults.SelectedItem;
			if (item != null)
			{
				// Check if the participant was added already
				bool exists = ranking.Results.Contains(item);
				Navigation.PushAsync(new UniversalAddModifyPage<CompetitionResult, CompetitionResult>(
					item, !exists, lager));
			}
			participantResults.SelectedItem = null;
		}

		void OnEditClickedParticipant(Participant participant)
		{
			Navigation.PushAsync(new NavigationPage(new AddEditParticipantPage(participant, false)));
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

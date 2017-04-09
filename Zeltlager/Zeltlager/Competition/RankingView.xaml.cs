using System;
using System.Collections.Generic;
using System.Linq;

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

		public bool Editable { get; set; }

		/// <summary>
		/// The ranking results including all not-ranked participants.
		/// </summary>
		List<CompetitionResult> totalRanking = new List<CompetitionResult>();

		public RankingView(LagerClient lager, Rankable rankable, bool editable = true)
		{
			this.lager = lager;
			this.rankable = rankable;
			ranking = rankable.Ranking;
			Editable = editable;
			InitializeComponent();
			BindingContext = this;

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
			totalRanking.Sort();

			// Set it to null first to refresh the list
			participantResults.ItemsSource = null;
			participantResults.ItemsSource = totalRanking;
		}

		void OnParticipantSelected(object sender, EventArgs e)
		{
			if (!Editable)
				return;

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

		async void OnIncreasingButtonClicked(object sender, EventArgs e)
		{
			await ranking.Rank(true);
			UpdateUI();
		}

		async void OnDecreasingButtonClicked(object sender, EventArgs e)
		{
			await ranking.Rank(false);
			UpdateUI();
		}
	}
}

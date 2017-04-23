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

		public bool Global { get; set; }
		public bool NotGlobal => !Global;

		/// <summary>
		/// The ranking results including all not-ranked participants.
		/// </summary>
		List<CompetitionResult> totalRanking = new List<CompetitionResult>();

		public Command OnEdit { get; set; }
		public Command OnDelete { get; set; }

		public RankingView(LagerClient lager, Rankable rankable, bool global = false,
			Action<CompetitionResult> onEdit = null, Func<CompetitionResult, Task> onDelete = null)
		{
			this.lager = lager;
			this.rankable = rankable;
			ranking = rankable.Ranking;
			Global = global;
			InitializeComponent();
			BindingContext = this;
			View paddingView = new ContentView();
			if (Device.OS == TargetPlatform.Android)
				paddingView.WidthRequest = 15;
			headerStack.Children.Add(paddingView);

			DataTemplate template = new DataTemplate(typeof(ParticipantResultCell));
			if (global)
			{
				OnEdit = new Command(sender => onEdit((CompetitionResult)sender));
				OnDelete = new Command(sender => onDelete((CompetitionResult)sender));
				template.SetBinding(ActionCell.OnEditCommandParameterProperty, new Binding("."));
				template.SetBinding(ActionCell.OnEditCommandProperty, new Binding(nameof(OnEdit), source: this));
				template.SetBinding(ActionCell.OnDeleteCommandParameterProperty, new Binding("."));
				template.SetBinding(ActionCell.OnDeleteCommandProperty, new Binding(nameof(OnDelete), source: this));
			}
			participantResults.ItemTemplate = template;
			
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
			if (Global)
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

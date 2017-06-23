using System;
using System.Linq;
using Xamarin.Forms;

namespace Zeltlager.Erwischt
{
	using Client;

	public class ErwischtStatisticsPage : TabbedPage
	{
		public class ErwischtParticipantStatisticsWrapper : IComparable<ErwischtParticipantStatisticsWrapper>, ISearchable
		{
			ErwischtParticipant participant;
			public string SearchableDetail { get; set; }
			public string SearchableText => participant.SearchableText;

			public ErwischtParticipantStatisticsWrapper(ErwischtParticipant participant)
			{
				this.participant = participant;
			}

			public int CompareTo(ErwischtParticipantStatisticsWrapper other)
			{
				int res = SearchableDetail.CompareTo(other.SearchableDetail);
				if (res != 0)
					return -res;
				return SearchableText.CompareTo(other.SearchableText);
			}
		}

		LagerClient lager;
		ErwischtGame currentGame;

		ContentPage currentGameContent;
		ContentPage allGamesContent;

		public ErwischtStatisticsPage(LagerClient lager, ErwischtGame currentGame) : base()
		{
			this.lager = lager;
			this.currentGame = currentGame;
			NavigationPage.SetBackButtonTitle(this, "");
			Style = (Style)Application.Current.Resources["TabbedPageStyle"];
			Title = "Statistiken";

			currentGameContent = new ContentPage()
			{
				Title = "aktuelles Spiel",
				Icon = Icons.GAMEPAD,
				Padding = new Thickness(8),
				Style = (Style)Application.Current.Resources["BaseStyle"],
			};
			Children.Add(currentGameContent);
			allGamesContent = new ContentPage()
			{
				Title = "alle Spiele",
				Icon = Icons.PODIUM,
				Padding = new Thickness(8),
				Style = (Style)Application.Current.Resources["BaseStyle"],
			};
			Children.Add(allGamesContent);
			UpdateUI();
		}

		void UpdateUI()
		{
			currentGameContent.Content = new SearchableListView<ErwischtParticipantStatisticsWrapper>
				(currentGame.ErwischtParticipants.Select(ep => new ErwischtParticipantStatisticsWrapper(ep)
				{
					SearchableDetail = ep.Catches.ToString()
				}).ToList(),
				null, null, null);
			allGamesContent.Content = new SearchableListView<ErwischtParticipantStatisticsWrapper>
				(lager.ErwischtHandler.Games.SelectMany(eg => eg.ErwischtParticipants).GroupBy(ep => ep.Member)
				.Select(grouping => new Tuple<ErwischtParticipant, int>(grouping.First(), grouping.Sum(ep => ep.Catches)))
				.Select(tuple => new ErwischtParticipantStatisticsWrapper(tuple.Item1)
				{
					SearchableDetail = tuple.Item2.ToString()
				}).ToList(),
				null, null, null);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}
	}
}

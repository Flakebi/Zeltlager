using System;
using System.Linq;
using Xamarin.Forms;
using Zeltlager.Client;

namespace Zeltlager.Erwischt
{
	public class ErwischtStatisticsPage : TabbedPage
	{
		public class ErwischtParticipantStatisticsWrapper : ISearchable
		{
			ErwischtParticipant participant;
			public string SearchableDetail { get; set; }
			public string SearchableText => participant.SearchableText;

			public ErwischtParticipantStatisticsWrapper(ErwischtParticipant participant)
			{
				this.participant = participant;
			}
		}

		LagerClient lager;

		public ErwischtStatisticsPage(LagerClient lager)
		{
			this.lager = lager;
            UpdateUI();
			NavigationPage.SetBackButtonTitle(this, "");
			Style = (Style)Application.Current.Resources["TabbedPageStyle"];
			Title = "Statistiken";
		}

		void UpdateUI()
		{
			Children.Clear(); 			Children.Add(new ContentPage()
			{
				Content = new SearchableListView<ErwischtParticipantStatisticsWrapper>
					(lager.ErwischtHandler.CurrentGame.ErwischtParticipants.Select(ep => new ErwischtParticipantStatisticsWrapper(ep)
					{
						SearchableDetail = ep.Catches.ToString()
					}).ToList(),
					 null, null, null),
				Title = "aktuelles Spiel",
				Icon = Icons.GAMEPAD,
				Padding = new Thickness(8),
				Style = (Style)Application.Current.Resources["BaseStyle"], 			}); 			Children.Add(new ContentPage()
			{
				Content = new SearchableListView<ErwischtParticipantStatisticsWrapper>
					(lager.ErwischtHandler.Games.SelectMany(eg => eg.ErwischtParticipants).GroupBy(ep => ep.Member)
					 .Select(grouping => new Tuple<ErwischtParticipant, int>(grouping.First(), grouping.Sum(ep => ep.Catches)))
					 .Select(tuple => new ErwischtParticipantStatisticsWrapper(tuple.Item1)
					 {
						 SearchableDetail = tuple.Item2.ToString()
					 }).ToList(), 					 null, null, null),
				Title = "alle Spiele",
				Icon = Icons.PODIUM,
				Padding = new Thickness(8),
				Style = (Style)Application.Current.Resources["BaseStyle"], 			});
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}
	}
}


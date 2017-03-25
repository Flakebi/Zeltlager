using System;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Zeltlager.Competition
{
    using Client;
    using UAM;

	public partial class CompetitionHandlerPage : ContentPage
	{
		LagerClient lager;

		public CompetitionHandlerPage(LagerClient lager)
		{
			InitializeComponent();
			this.lager = lager;
			Content = new SearchableListView<Competition>(lager.CompetitionHandler.Competitions.Where(c => c.IsVisible).ToList(),
			                                              OnEditClicked, OnDeleteClicked, OnCompetitionClick);
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnAddClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Competition, Rankable>
			                                             (new Competition(null, "", lager), true, lager)),true);
		}

		void OnEditClicked(Competition comp)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Competition, Rankable>(comp,false, lager)));
		}

		async Task OnDeleteClicked(Competition comp)
		{
			await comp.Delete(lager);
			OnAppearing();
		}

		void OnCompetitionClick(Competition comp)
		{
			Navigation.PushAsync(new CompetitionPage(comp, lager));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Content = new SearchableListView<Competition>(lager.CompetitionHandler.Competitions.Where(c => c.IsVisible).ToList(),
			                                              OnEditClicked, OnDeleteClicked, OnCompetitionClick);
		}
	}
}

using System;
using System.Linq;

using Xamarin.Forms;
using System.Threading.Tasks;

namespace Zeltlager.General
{
	using Client;
	using UAM;

	public partial class MembersPage : ContentPage
	{
		LagerClient lager;

		public MembersPage(LagerClient lager)
		{
			InitializeComponent();
			Padding = new Thickness(10);
			this.lager = lager;
			NavigationPage.SetBackButtonTitle(this, "");
			UpdateUI();
		}

		void UpdateUI()
		{
			// TODO: think about what should happen if member is clicked
			Content = new SearchableListView<Member>(lager.VisibleMembers.Where(m => m.IsVisible).ToList(),
													 OnEditClicked, OnDeleteClicked, null);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			if (!lager.VisibleTents.Any())
			{
				DisplayAlert("Keine Zelte vorhanden", "Bitte füge ein Zelt hinzu. Jeder Teilnehmer muss ein Zelt haben.", "Ok");
				return;
			}
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Member, Member>
			                                             (new Member(lager), true, lager)), true);
		}

		void OnEditClicked(Member member)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Member, Member>
			                                             (member, false, lager)), true);
		}

		async Task OnDeleteClicked(Member member)
		{
			await member.Delete(lager);
			OnAppearing();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}
	}
}

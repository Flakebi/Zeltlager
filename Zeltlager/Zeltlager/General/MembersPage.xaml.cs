using System;
using System.Linq;

using Xamarin.Forms;

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
			// TODO: think about what should happen if member is clicked
			Content = new SearchableListView<Member>(lager.Members, OnContextActionEdit, OnContextActionDelete, null);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			if (!lager.Tents.Any())
			{
				DisplayAlert("Keine Zelte vorhanden", "Bitte füge ein Zelt hinzu. Jeder Teilnehmer muss ein Zelt haben.", "Ok");
				return;
			}
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Member, Member>(new Member(lager), true, lager)), true);
		}

		void OnContextActionEdit(Member member)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Member, Member>(member, false, lager)), true);
		}

		async void OnContextActionDelete(Member member)
		{
			//TODO Revert packets
			//await lager.AddPacket(new DeleteMember(member));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Content = new SearchableListView<Member>(lager.Members, OnContextActionEdit, OnContextActionDelete, null);
		}
	}
}

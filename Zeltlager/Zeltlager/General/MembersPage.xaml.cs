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
			this.lager = lager;
			Content = new SearchableListView<Member>(lager.Members, OnContextActionEdit, OnContextActionDelete);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			if (!lager.Tents.Any())
			{
				DisplayAlert("Keine Zelte vorhanden", "Bitte füge ein Zelt hinzu. Jeder Teilnehmer muss ein Zelt haben.", "Ok");
				return;
			}
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Member>(new Member(), true, lager)), true);
		}

		void OnContextActionEdit(Member member)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Member>(member, false, lager)), true);
		}

		async void OnContextActionDelete(Member member)
		{
			//TODO Revert packets
			//await lager.AddPacket(new DeleteMember(member));
		}
	}
}

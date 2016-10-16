using System;
using System.Linq;
using Zeltlager.DataPackets;

using Xamarin.Forms;

namespace Zeltlager.General
{
	using Client;
	using DataPackets;
	using UAM;

	public partial class MembersPage : ContentPage
	{
		public MembersPage()
		{
			InitializeComponent();
			Content = new SearchableListView<Member>(LagerClient.CurrentLager.Members, OnContextActionEdit, OnContextActionDelete);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			if (!LagerClient.CurrentLager.Tents.Any())
			{
				DisplayAlert("Keine Zelte vorhanden", "Bitte füge ein Zelt hinzu. Jeder Teilnehmer muss ein Zelt haben.", "Ok");
				return;
			}
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Member>(new Member(), true)));
		}

		void OnContextActionEdit(Member member)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Member>(member, false)), true);
		}

		async void OnContextActionDelete(Member member)
		{
			await LagerClient.CurrentLager.AddPacket(new DeleteMember(member));
		}
	}
}

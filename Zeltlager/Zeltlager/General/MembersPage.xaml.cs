using System;
using Xamarin.Forms;
using Zeltlager.UAM;
using System.Linq;

namespace Zeltlager.General
{
	using Client;

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

		void OnContextActionEdit(object sender)
		{
			//TODO
			//Tent t = (Tent)((MenuItem)sender).CommandParameter;
			//Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Tent>(t, false)), true);
		}

		async void OnContextActionDelete(object sender)
		{
			//TODO
			//await LagerClient.CurrentLager.AddPacket(new DeleteTent((Tent)((MenuItem)sender).CommandParameter));
		}
	}
}

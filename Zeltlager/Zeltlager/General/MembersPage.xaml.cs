using System;
using Xamarin.Forms;
using Zeltlager.UAM;

namespace Zeltlager.General
{
	public partial class MembersPage : ContentPage
	{
		public MembersPage()
		{
			InitializeComponent();
			Content = new SearchableListView<Member>(Lager.CurrentLager.Members);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Member>(new Member(), true)));
		}
	}
}

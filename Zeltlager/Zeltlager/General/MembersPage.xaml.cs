using System;
using Xamarin.Forms;

namespace Zeltlager
{
	public partial class MembersPage : ContentPage
	{
		public MembersPage()
		{
			InitializeComponent();
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Member>(new Member(), true)));
		}
	}
}

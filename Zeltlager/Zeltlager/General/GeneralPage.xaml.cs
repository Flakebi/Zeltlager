using System;

using Xamarin.Forms;
using Zeltlager.Client;

namespace Zeltlager.General
{
	public partial class GeneralPage : ContentPage
	{
		LagerClient lager;
		public GeneralPage(LagerClient lager)
		{
			InitializeComponent();
			this.lager = lager;
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnMemberClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new MembersPage(lager));
		}

		void OnTentClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new TentsPage(lager));
		}
	}
}

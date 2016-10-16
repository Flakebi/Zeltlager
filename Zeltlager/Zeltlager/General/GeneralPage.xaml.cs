using System;

using Xamarin.Forms;

namespace Zeltlager.General
{
	public partial class GeneralPage : ContentPage
	{
		public GeneralPage()
		{
			InitializeComponent();
		}

		void OnMemberClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new MembersPage());
		}

		void OnTentClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new TentsPage());
		}
	}
}

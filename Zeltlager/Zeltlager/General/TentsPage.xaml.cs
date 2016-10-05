using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager
{
	public partial class TentsPage : ContentPage
	{
		public TentsPage()
		{
			InitializeComponent();
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Tent>(new Tent(), true)));
		}
	}
}


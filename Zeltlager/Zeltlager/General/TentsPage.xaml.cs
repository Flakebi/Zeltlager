using System;
using Zeltlager.UAM;

using Xamarin.Forms;

namespace Zeltlager.General
{
	public partial class TentsPage : ContentPage
	{
		public TentsPage()
		{
			InitializeComponent();
			Content = new SearchableListView<Tent>(Lager.CurrentLager.Tents);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Tent>(new Tent(), true)));
		}
	}
}


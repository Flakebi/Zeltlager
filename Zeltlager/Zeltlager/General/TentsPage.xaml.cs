using System;
using Zeltlager.UAM;

using Xamarin.Forms;
using System.Linq;
using System.Collections.Generic;

namespace Zeltlager.General
{
	using Client;

	public partial class TentsPage : ContentPage
	{
		public TentsPage()
		{
			InitializeComponent();
			Content = new SearchableListView<Tent>(LagerClient.CurrentLager.Tents);
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			var tentNumber = (byte) (LagerClient.CurrentLager.Tents.Max(t => t.Number) + 1);
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<Tent>(new Tent(new TentId(),tentNumber, "", new List<Member>()),true)));
		}
	}
}


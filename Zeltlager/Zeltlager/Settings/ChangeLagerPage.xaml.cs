using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.Client;
using System.Linq;

namespace Zeltlager.Settings
{
	public partial class ChangeLagerPage : ContentPage
	{
		// current (old) lager
		LagerClient lager;

		public ChangeLagerPage(LagerClient lager)
		{
			InitializeComponent();
			this.lager = lager;
			NavigationPage.SetBackButtonTitle(this, "");
			Content = new SearchableListView<LagerClient>(lager.Manager.Lagers.Values.Cast<LagerClient>().ToList(),
			                                              null, null, OnLagerClicked);
		}

		void OnLagerClicked(LagerClient newlager)
		{
			((App)Application.Current).ChangeLager(lager, newlager);
		}
	}
}

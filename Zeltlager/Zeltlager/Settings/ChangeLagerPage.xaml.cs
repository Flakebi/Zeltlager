using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.Client;
using System.Linq;

namespace Zeltlager.Settings
{
	public partial class ChangeLagerPage : ContentPage
	{
		LagerClientManager manager;

		public ChangeLagerPage(LagerClientManager manager)
		{
			InitializeComponent();
			this.manager = manager;
			NavigationPage.SetBackButtonTitle(this, "");
			//Content = new SearchableListView<LagerClient>(manager.Lagers.Values.Cast<LagerClient>().ToList(),
			                                              //null, null, OnLagerClicked);
		}

		void OnLagerClicked(LagerClient lager)
		{
			
		}
	}
}

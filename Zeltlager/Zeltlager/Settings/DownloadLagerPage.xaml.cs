using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Settings
{
	public partial class DownloadLagerPage : ContentPage
	{
		public DownloadLagerPage()
		{
			InitializeComponent();
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnRequestClicked(object sender, EventArgs e)
		{
			string password = PasswordEntry.Text;
			// TODO request matching lagers from Server
			//IReadOnlyList<Lager> lagers = 
			//vsl.Children.Add(new SearchableListView)
		}
	}
}

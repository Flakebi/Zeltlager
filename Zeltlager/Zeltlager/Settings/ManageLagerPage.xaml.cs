using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.Client;

namespace Zeltlager.Settings
{
	public partial class ManageLagerPage : ContentPage
	{
		LagerClient lager;

		public ManageLagerPage(LagerClient lager)
		{
			InitializeComponent();
			this.lager = lager;
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnCreateLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new CreateLagerPage(), true);
		}

		void OnDownloadLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new DownloadLagerPage(lager), true);
		}

		void OnChangeLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ChangeLagerPage(lager), true);
		}

		void OnUploadLagerClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new UploadLagerPage(lager), true);
		}
	}
}

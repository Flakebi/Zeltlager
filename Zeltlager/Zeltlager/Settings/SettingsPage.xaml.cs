using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.Client;

namespace Zeltlager.Settings
{
	public partial class SettingsPage : ContentPage
	{
		public SettingsPage()
		{
			InitializeComponent();
		}

		void OnLogClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new LogPage());
		}
	}
}

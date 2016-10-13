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
			logLabel.Text = LagerBase.Log.printLog();
		}
	}
}

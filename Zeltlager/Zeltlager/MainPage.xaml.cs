using System;
using Xamarin.Forms;
using Zeltlager.General;
using Zeltlager.Client;

namespace Zeltlager
{
	public partial class MainPage : ContentPage
	{
		LagerClient lager;

		public MainPage(LagerClient lager)
		{
			InitializeComponent();
			this.lager = lager;
			NavigationPage.SetBackButtonTitle(this, "");
			BindingContext = this;
		}

		public string LagerName => lager.Data.Name;

		void OnSynchronizeClicked(object sender, EventArgs e)
		{
		}

		void OnCompetitionClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Competition.CompetitionHandlerPage(lager));
		}

		void OnErwischtClicked(object sender, EventArgs e)
		{
		}

		void OnCalendarClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Calendar.CalendarPage(lager));
		}

		void OnGeneralClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new GeneralPage(lager));
		}

		void OnSettingsClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Settings.SettingsPage(lager));
		}
	}
}

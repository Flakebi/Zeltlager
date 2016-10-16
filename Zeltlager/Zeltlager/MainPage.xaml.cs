using System;
using Xamarin.Forms;
using Zeltlager.General;

namespace Zeltlager
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		void OnSynchronizeClicked(object sender, EventArgs e)
		{
		}

		void OnCompetitionClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Competition.CompetitionHandlerPage());
		}

		void OnErwischtClicked(object sender, EventArgs e)
		{
		}

		void OnCalendarClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Calendar.CalendarPage());
		}

		void OnGeneralClicked(object sender, EventArgs e) => Navigation.PushAsync(new GeneralPage());

		void OnSettingsClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new Settings.SettingsPage());
		}
	}
}

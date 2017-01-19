using System;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	using Client;
	using UAM;

	public partial class CalendarPage : CarouselPage
	{
		LagerClient lager;

		public CalendarPage(LagerClient lager)
		{
			this.lager = lager;
			InitializeComponent();
			NavigationPage.SetBackButtonTitle(this, "");
			UpdateUI();
		}

		void UpdateUI()
		{
			var days = lager.Calendar.Days;
			foreach (Day day in days)
			{
				DayPage dp = new DayPage(day, lager);
				Children.Add(dp);
			}
			foreach (ContentPage cp in Children)
				((DayPage)cp).removeNavButtons();
			lager.Calendar.IncludeStandardEvents();
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<CalendarEvent, PlannedCalendarEvent>
			           (new CalendarEvent(null, ((DayPage)CurrentPage).Day.Date, "", "", lager), true, lager)), true);
		}

		void OnRecurrentButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new StandardEventsPage(lager.Calendar));
		}

		void OnPlannedButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new PlannedEventsPage(lager.Calendar));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}
	}
}

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
			var days = lager.Calendar.Days;
			foreach (Day day in days)
			{
				DayPage dp = new DayPage(day, lager);
				Children.Add(dp);
			}
			foreach (ContentPage cp in Children)
				((DayPage)cp).removeNavButtons();
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			//Navigation.PushModalAsync(new NavigationPage(new CalendarEventEditPage(new CalendarEvent(((DayPage)CurrentPage).Day.Date, ""))));
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<CalendarEvent>(new CalendarEvent(null, ((DayPage)CurrentPage).Day.Date, "", "", lager), true, lager)), true);
		}
	}
}

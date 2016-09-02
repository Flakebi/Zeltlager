using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	public partial class CalendarPage : CarouselPage
	{
		public CalendarPage()
		{
			InitializeComponent();
			var days = Lager.CurrentLager.Calendar.Days;
			foreach (Day day in days)
			{
				DayPage dp = new DayPage(day);
				Children.Add(dp);
			}
			foreach (ContentPage cp in Children) {
				((DayPage)cp).removeNavButtons();
			}
			Title = "Kalender";
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new CalendarEventEditPage(new CalendarEvent(((DayPage)CurrentPage).Day.Date, "")));
		}
	}
}

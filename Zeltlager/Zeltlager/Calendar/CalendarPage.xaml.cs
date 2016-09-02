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
			var days = Calendar.Cal.Days;
			foreach (Day day in days)
			{
				Children.Add(new DayPage(day));
			}
			Title = "Kalender";
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new CalendarEventEditPage(new CalendarEvent(DateTime.Now, "")));
		}
	}
}

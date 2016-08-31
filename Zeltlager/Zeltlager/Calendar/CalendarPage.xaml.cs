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
			ToolbarItems.Add(new ToolbarItem("Add", "Add.png", async () => { await Navigation.PushModalAsync(new CalendarEventEditPage(new CalendarEvent(DateTime.Now, ""))); }, ToolbarItemOrder.Primary, 10));
		}
	}
}

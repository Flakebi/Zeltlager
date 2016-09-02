using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	public partial class CalendarEventEditPage : ContentPage
	{
		public CalendarEvent CalendarEvent;

		public CalendarEventEditPage(CalendarEvent eventToEdit)
		{
			InitializeComponent();
			CalendarEvent = eventToEdit;
			BindingContext = CalendarEvent;
			ToolbarItems.Add(new ToolbarItem("Add", "Zeltlager.add.png", () => Navigation.PushAsync(new CalendarEventEditPage(new CalendarEvent(DateTime.Now, ""))), ToolbarItemOrder.Default, 1));

		}

		public void OnSaveClicked(object sender, EventArgs e)
		{
			//Insert Calendar Event into correct day
			Lager.CurrentLager.Calendar.InsertNewCalendarEvent(CalendarEvent);
			Navigation.PopAsync();
		}
	}
}

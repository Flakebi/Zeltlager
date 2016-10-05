using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	public partial class CalendarEventEditPage : ContentPage
	{
		public CalendarEvent CalendarEvent;

		private CalendarEvent newCalendarEvent;

		public CalendarEventEditPage(CalendarEvent eventToEdit)
		{
			InitializeComponent();
			CalendarEvent = eventToEdit;
			newCalendarEvent = eventToEdit.CloneDeep();
			BindingContext = newCalendarEvent;
		}

		void OnSaveClicked(object sender, EventArgs e)
		{
			//Delete Item
			Lager.CurrentLager.Calendar.RemoveCalendarEvent(CalendarEvent);
			//Insert Calendar Event into correct day
			Lager.CurrentLager.Calendar.InsertNewCalendarEvent(newCalendarEvent);
			Navigation.PopModalAsync();
		}

		void OnCancelClicked(object sender, EventArgs e)
		{
			Navigation.PopModalAsync();
		}
	}
}

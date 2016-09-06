using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	public partial class CalendarEventEditPage : ContentPage
	{
		public CalendarEvent CalendarEvent;

		//if editing is cancelled
		private CalendarEvent oldCalendarEvent;

		public CalendarEventEditPage(CalendarEvent eventToEdit)
		{
			InitializeComponent();
			CalendarEvent = eventToEdit;
			oldCalendarEvent = eventToEdit.Clone();
			BindingContext = CalendarEvent;
			//Delete Item
			Lager.CurrentLager.Calendar.RemoveCalendarEvent(eventToEdit);
		}

		void OnSaveClicked(object sender, EventArgs e)
		{
			//Insert Calendar Event into correct day
			Lager.CurrentLager.Calendar.InsertNewCalendarEvent(CalendarEvent);
			Navigation.PopModalAsync();
		}

		void OnCancelClicked(object sender, EventArgs e)
		{
			Lager.CurrentLager.Calendar.InsertNewCalendarEvent(oldCalendarEvent);
			Navigation.PopModalAsync();
		}
	}
}

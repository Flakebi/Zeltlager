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
			//titleEntry.Text = CalendarEvent.title;
			//detailEntry.Text = CalendarEvent.detail;
		}

		public void OnSaveClicked(object sender, EventArgs e)
		{
			Navigation.PopAsync();
		}
	}
}

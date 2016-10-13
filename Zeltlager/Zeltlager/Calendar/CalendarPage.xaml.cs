﻿using System;
using Zeltlager.UAM;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	using Client;

	public partial class CalendarPage : CarouselPage
	{
		public CalendarPage()
		{
			InitializeComponent();
			var days = LagerClient.CurrentLager.Calendar.Days;
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
			//Navigation.PushModalAsync(new NavigationPage(new CalendarEventEditPage(new CalendarEvent(((DayPage)CurrentPage).Day.Date, ""))));
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<CalendarEvent>(new CalendarEvent(((DayPage)CurrentPage).Day.Date, "", ""), true)), true);
		}
	}
}

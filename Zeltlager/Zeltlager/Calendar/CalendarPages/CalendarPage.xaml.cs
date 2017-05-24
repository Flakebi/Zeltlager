using System;
using System.Linq;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	using Client;
	using UAM;

	public partial class CalendarPage : CarouselPage
	{
		LagerClient lager;

		bool inUpdateUI = false;

		public CalendarPage(LagerClient lager)
		{
			this.lager = lager;
			InitializeComponent();
			NavigationPage.SetBackButtonTitle(this, "");
			if (lager.Calendar.Days.Any())
			{
				// show Day nearest to Today first
				inUpdateUI = true;
				DayPage dp = new DayPage(lager.Calendar.FindClosestDayToNow(), lager);
				Children.Insert(0, dp);
				CurrentPage = dp;
				inUpdateUI = false;
			}
			UpdateUI();
		}

		void UpdateUI()
		{
			if (inUpdateUI)
				return;
			inUpdateUI = true;

			// insert Day pages for days near the currently selected one
			foreach (Day day in lager.Calendar.Days)
			{
				int index = Children.TakeWhile(dp => ((DayPage)dp).Day.Date < day.Date).Count();
				// (Seite schon drin) oder zu weit von aktueller Page weg
				if ((index < Children.Count && ((DayPage)Children[index]).Day.Date == day.Date) || Math.Abs(Children.IndexOf(CurrentPage) - index) > 1)
				{
					continue;
				}
				Children.Insert(index, new DayPage(day, lager));
			}

			lager.Calendar.IncludeStandardEvents();

			// check wheter there is a day page which day is not in the calendar anymore
			foreach(ContentPage p in Children)
			{
				DayPage dp = p as DayPage;
				if (dp != null && !lager.Calendar.Days.Contains(dp.Day))
				{
					Children.Remove(p);
				}
			}

			inUpdateUI = false;
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			if (CurrentPage != null)
			{
				Navigation.PushAsync(new UniversalAddModifyPage<CalendarEvent, PlannedCalendarEvent>
					(new CalendarEvent(null, ((DayPage)CurrentPage).Day.Date.Add(DateTime.Now.TimeOfDay), "", "", lager), true, lager));
			}
			else
			{
				Navigation.PushAsync(new UniversalAddModifyPage<CalendarEvent, PlannedCalendarEvent>
					(new CalendarEvent(null, DateTime.Now, "", "", lager), true, lager));
			}
		}

		void OnRecurrentButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new StandardEventsPage(lager));
		}

		void OnPlannedButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new PlannedEventsPage(lager));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}

		protected override void OnCurrentPageChanged()
		{
			base.OnCurrentPageChanged();
			UpdateUI();
			((DayPage)CurrentPage)?.UpdateUI();
			((DayPage)CurrentPage)?.RemoveNavButtons();
		}
	}
}

using System;

using Xamarin.Forms;
using System.Linq;

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
			UpdateUI();
		}

		void UpdateUI()
		{
			if (inUpdateUI)
				return;
			inUpdateUI = true;
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
			inUpdateUI = false;
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			if (CurrentPage != null)
			{
				Navigation.PushAsync(new UniversalAddModifyPage<CalendarEvent, PlannedCalendarEvent>
						   (new CalendarEvent(null, ((DayPage)CurrentPage).Day.Date, "", "", lager), true, lager));
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
			((DayPage)CurrentPage).UpdateUI();
			((DayPage)CurrentPage).RemoveNavButtons();
		}
	}
}

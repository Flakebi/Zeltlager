using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.UAM;
using System.Linq;

namespace Zeltlager.Calendar
{
	public partial class PlannedEventsPage : ContentPage
	{
		Calendar calendar;
		public Command OnEdit { get; set; }
		public Command OnDelete { get; set; }

		public PlannedEventsPage(Calendar c)
		{
			InitializeComponent();
			calendar = c;
			BindingContext = calendar;

			var dataTemplate = new DataTemplate(typeof(GeneralCalendarEventCell));
			OnEdit = new Command(sender => OnEditClicked((PlannedCalendarEvent)sender));
			OnDelete = new Command(sender => OnDeleteClicked((PlannedCalendarEvent)sender));

			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandProperty, new Binding(nameof(OnEdit), source: this));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandProperty, new Binding(nameof(OnDelete), source: this));

			ListView calendarEventList = new ListView
			{
				ItemsSource = calendar.PlannedEvents.Where(x => x.IsShown),
				ItemTemplate = dataTemplate,
				BindingContext = calendar.PlannedEvents,
			};
			calendarEventList.ItemSelected += ((sender, e) =>
			{
				if (calendarEventList.SelectedItem != null)
				{
					OnPlannedEventClicked((PlannedCalendarEvent)calendarEventList.SelectedItem);
					calendarEventList.SelectedItem = null;
				}
			});

			Content = calendarEventList;
			Style = (Style)Application.Current.Resources["BaseStyle"];
			NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnAddClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<PlannedCalendarEvent, PlannedCalendarEvent>
			           (new PlannedCalendarEvent(null, "", "", calendar.GetLager()), true, calendar.GetLager())), true);
		}

		void OnEditClicked(PlannedCalendarEvent pce)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<PlannedCalendarEvent, PlannedCalendarEvent>
					   (pce, false, calendar.GetLager())), true);
		}

		void OnDeleteClicked(PlannedCalendarEvent pce)
		{
			//TODO revert packages
		}

		void OnPlannedEventClicked(PlannedCalendarEvent pce)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<CalendarEvent, PlannedCalendarEvent>
			           (new ExPlCalendarEvent(pce), true, pce.GetLager())), true);
		}
	}
}

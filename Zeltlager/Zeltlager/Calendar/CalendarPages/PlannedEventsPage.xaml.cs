using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.UAM;

namespace Zeltlager.Calendar
{
	public partial class PlannedEventsPage : ContentPage
	{
		Calendar calendar;
		Command onEdit, onDelete;

		public PlannedEventsPage(Calendar c)
		{
			InitializeComponent();
			calendar = c;
			BindingContext = calendar;

			var dataTemplate = new DataTemplate(typeof(GeneralCalendarEventCell));
			onEdit = new Command(sender => OnEditClicked((PlannedCalendarEvent)sender));
			onDelete = new Command(sender => OnDeleteClicked((PlannedCalendarEvent)sender));

			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandProperty, new Binding(nameof(onEdit), source: this));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandProperty, new Binding(nameof(onDelete), source: this));

			ListView calendarEventList = new ListView
			{
				ItemsSource = calendar.PlannedEvents,
				ItemTemplate = dataTemplate,
				BindingContext = calendar.PlannedEvents,
			};

			Content = calendarEventList;
			Style = (Style)Application.Current.Resources["BaseStyle"];
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
	}
}

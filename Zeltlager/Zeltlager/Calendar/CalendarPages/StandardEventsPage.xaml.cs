using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.UAM;

namespace Zeltlager.Calendar
{
	public partial class StandardEventsPage : ContentPage
	{
		Calendar calendar;

		public StandardEventsPage(Calendar c)
		{
			InitializeComponent();
			calendar = c;
			BindingContext = calendar;

			var dataTemplate = new DataTemplate(typeof(GeneralCalendarEventCell));
			var onEdit = new Command(sender => OnEditClicked((StandardCalendarEvent) sender));
			var onDelete = new Command(sender => OnDeleteClicked((StandardCalendarEvent) sender));

			dataTemplate.SetBinding(SearchableCell.OnEditCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(SearchableCell.OnEditCommandProperty, new Binding(nameof(onEdit), source: this));
			dataTemplate.SetBinding(SearchableCell.OnDeleteCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(SearchableCell.OnDeleteCommandProperty, new Binding(nameof(onDelete), source: this));

			ListView calendarEventList = new ListView
			{
				ItemsSource = calendar.StandardEvents,
				ItemTemplate = dataTemplate,
				BindingContext = calendar.StandardEvents,
			};

			Content = calendarEventList;
			Style = (Style)Application.Current.Resources["BaseStyle"];
		}

		void OnAddClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<StandardCalendarEvent, PlannedCalendarEvent>
			           (new StandardCalendarEvent(null, new TimeSpan(), "", "", calendar.GetLager()), true, calendar.GetLager())), true);
		}

		void OnEditClicked(StandardCalendarEvent sce)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<StandardCalendarEvent, PlannedCalendarEvent>
			           (sce, false, calendar.GetLager())), true);
		}

		void OnDeleteClicked(StandardCalendarEvent sce)
		{
			//TODO revert packages
		}
	}
}

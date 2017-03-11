using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.UAM;

namespace Zeltlager.Calendar
{
	public partial class StandardEventsPage : ContentPage
	{
		Calendar calendar;
		public Command OnEdit { get; set; }
		public Command OnDelete { get; set; }

		public StandardEventsPage(Calendar c)
		{
			InitializeComponent();
			calendar = c;
			BindingContext = calendar;

			var dataTemplate = new DataTemplate(typeof(GeneralCalendarEventCell));
			OnEdit = new Command(sender => OnEditClicked((StandardCalendarEvent) sender));
			OnDelete = new Command(sender => OnDeleteClicked((StandardCalendarEvent) sender));

			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandProperty, new Binding(nameof(OnEdit), source: this));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandProperty, new Binding(nameof(OnDelete), source: this));

			ListView calendarEventList = new ListView
			{
				ItemsSource = calendar.StandardEvents,
				ItemTemplate = dataTemplate,
				BindingContext = calendar.StandardEvents,
			};
			calendarEventList.ItemSelected +=
				((sender, e) => calendarEventList.SelectedItem = null);

			Content = calendarEventList;
			Style = (Style)Application.Current.Resources["BaseStyle"];
			NavigationPage.SetBackButtonTitle(this, "");
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
			sce.IsVisible = false;
		}
	}
}

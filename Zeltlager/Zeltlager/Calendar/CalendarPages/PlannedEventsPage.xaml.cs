using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.UAM;
using System.Linq;
using System.Threading.Tasks;
using Zeltlager.Client;

namespace Zeltlager.Calendar
{
	public partial class PlannedEventsPage : ContentPage
	{
		LagerClient lager;
		public Command OnEdit { get; set; }
		public Command OnDelete { get; set; }

		public PlannedEventsPage(LagerClient lager)
		{
			InitializeComponent();
			this.lager = lager;
			BindingContext = lager.Calendar;

			Style = (Style)Application.Current.Resources["BaseStyle"];
			NavigationPage.SetBackButtonTitle(this, "");
			UpdateUI();
		}

		void UpdateUI()
		{
			var dataTemplate = new DataTemplate(typeof(GeneralCalendarEventCell));
			OnEdit = new Command(sender => OnEditClicked((PlannedCalendarEvent)sender));
			OnDelete = new Command(async sender => await OnDeleteClicked((PlannedCalendarEvent)sender));

			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandProperty, new Binding(nameof(OnEdit), source: this));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandProperty, new Binding(nameof(OnDelete), source: this));

			ListView calendarEventList = new ListView
			{
				ItemsSource = lager.Calendar.PlannedEvents.Where(x => x.IsVisible),
				ItemTemplate = dataTemplate,
				BindingContext = lager.Calendar.PlannedEvents,
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
		}

		void OnAddClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new UniversalAddModifyPage<PlannedCalendarEvent, PlannedCalendarEvent>
					   (new PlannedCalendarEvent(null, "", "", lager), true, lager));
		}

		void OnEditClicked(PlannedCalendarEvent pce)
		{
			Navigation.PushAsync(new UniversalAddModifyPage<PlannedCalendarEvent, PlannedCalendarEvent>(pce, false, lager));
		}

		async Task OnDeleteClicked(PlannedCalendarEvent pce)
		{
			await pce.Delete(lager);
			OnAppearing();
		}

		void OnPlannedEventClicked(PlannedCalendarEvent pce)
		{
			Navigation.PushAsync(new UniversalAddModifyPage<ExPlCalendarEvent, PlannedCalendarEvent>
					   (new ExPlCalendarEvent(pce), true, lager));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}
	}
}

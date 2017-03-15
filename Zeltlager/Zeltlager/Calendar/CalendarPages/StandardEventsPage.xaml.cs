using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.UAM;
using System.Linq;
using Zeltlager.Client;
using System.Threading.Tasks;

namespace Zeltlager.Calendar
{
	public partial class StandardEventsPage : ContentPage
	{
		LagerClient lager;
		public Command OnEdit { get; set; }
		public Command OnDelete { get; set; }

		public StandardEventsPage(LagerClient lager)
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
			OnEdit = new Command(sender => OnEditClicked((StandardCalendarEvent)sender));
			OnDelete = new Command(async sender => await OnDeleteClicked((StandardCalendarEvent)sender));

			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandProperty, new Binding(nameof(OnEdit), source: this));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandProperty, new Binding(nameof(OnDelete), source: this));

			ListView calendarEventList = new ListView
			{
				ItemsSource = lager.Calendar.StandardEvents.Where(se => se.IsVisible),
				ItemTemplate = dataTemplate,
				BindingContext = lager.Calendar.StandardEvents,
			};
			calendarEventList.ItemSelected +=
				((sender, e) => calendarEventList.SelectedItem = null);
			Content = calendarEventList;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}

		void OnAddClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<StandardCalendarEvent, PlannedCalendarEvent>
					   (new StandardCalendarEvent(null, new TimeSpan(), "", "", lager), true, lager)), true);
		}

		void OnEditClicked(StandardCalendarEvent sce)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<StandardCalendarEvent, PlannedCalendarEvent>
			           (sce, false, lager)), true);
		}

		async Task OnDeleteClicked(StandardCalendarEvent sce)
		{
			await sce.Delete(lager);
			OnAppearing();
		}
	}
}

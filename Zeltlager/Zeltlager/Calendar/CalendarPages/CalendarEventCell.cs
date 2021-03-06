using System;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	using Client;
	using UAM;

	public class CalendarEventCell : ViewCell
	{
		Label time = new Label
		{
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalTextAlignment = TextAlignment.End,
			TextColor = (Color)Application.Current.Resources["whiteColor"]
		};
		Label title = new Label
		{
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalTextAlignment = TextAlignment.End
		};
		Label detail = new Label
		{
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalTextAlignment = TextAlignment.End,
			TextColor = (Color)Application.Current.Resources["textColorSecondary"]
		};

		public CalendarEventCell()
		{
			StackLayout horizontalLayout = new StackLayout();

			horizontalLayout.Padding = new Thickness(10, 0);

			//set bindings
			time.SetBinding(Label.TextProperty, "TimeString");
			title.SetBinding(Label.TextProperty, "Title");
			detail.SetBinding(Label.TextProperty, "Detail");


			horizontalLayout.Orientation = StackOrientation.Horizontal;
			time.HorizontalOptions = LayoutOptions.Start;

			horizontalLayout.Children.Add(time);
			horizontalLayout.Children.Add(title);
			horizontalLayout.Children.Add(detail);

			var editAction = new MenuItem { Icon = Icons.EDIT, Text = Icons.EDIT_TEXT };
			editAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
			editAction.Clicked += OnEdit;

			var deleteAction = new MenuItem { Icon = Icons.DELETE, Text = Icons.DELETE_TEXT, IsDestructive = true };
			deleteAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
			deleteAction.Clicked += OnDelete;

			ContextActions.Add(editAction);
			ContextActions.Add(deleteAction);
			View = horizontalLayout;
		}

		void OnEdit(object sender, EventArgs e)
		{
			CalendarEvent ce = (CalendarEvent)((MenuItem)sender).CommandParameter;
			//Call edit screen for item
			((View)Parent).Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<CalendarEvent, PlannedCalendarEvent>(ce, false, ce.GetLager())), true);
		}

		async void OnDelete(object sender, EventArgs e)
		{
			//TODO Revert packets
			//await lager.AddPacket(new DeleteCalendarEvent((CalendarEvent)((MenuItem)sender).CommandParameter));
		}
	}
}

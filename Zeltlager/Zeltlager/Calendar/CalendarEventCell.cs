using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	public class CalendarEventCell : ViewCell
	{

		Label time = new Label()
		{
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalTextAlignment = TextAlignment.End
		};
		Label title = new Label()
		{
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalTextAlignment = TextAlignment.End
		};
		Label detail = new Label()
		{
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalTextAlignment = TextAlignment.End
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

			var editAction = new MenuItem { Text = "Bearbeiten" };
			editAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
			editAction.Clicked += OnEdit;

			var deleteAction = new MenuItem { Text = "Löschen", IsDestructive = true };
			deleteAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
			deleteAction.Clicked += OnDelete;

			ContextActions.Add(editAction);
			ContextActions.Add(deleteAction);
			View = horizontalLayout;
		}

		private void OnEdit(object sender, EventArgs e)
		{
			//Call edit screen for item
			((View)Parent).Navigation.PushAsync(new CalendarEventEditPage((CalendarEvent)((MenuItem)sender).CommandParameter));
		}

		private void OnDelete(object sender, EventArgs e)
		{
			Lager.CurrentLager.Calendar.RemoveCalendarEvent((CalendarEvent)((MenuItem)sender).CommandParameter);
		}
	}
}

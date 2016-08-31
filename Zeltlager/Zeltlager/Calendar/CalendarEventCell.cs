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

			ContextActions.Add(editAction);
			View = horizontalLayout;
		}

		private void OnEdit(object sender, EventArgs e)
		{
			//Call edit screen for item
			ParentView.Navigation.PushAsync(new CalendarEventEditPage((CalendarEvent)((MenuItem)sender).CommandParameter));
		}
	}
}

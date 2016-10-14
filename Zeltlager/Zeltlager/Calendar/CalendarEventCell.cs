using System;
using Zeltlager.UAM;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	using Client;

	public class CalendarEventCell : ViewCell
	{
		Label time = new Label()
		{
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalTextAlignment = TextAlignment.End,
			TextColor = (Color)Application.Current.Resources["whiteColor"]
		};
		Label title = new Label()
		{
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalTextAlignment = TextAlignment.End
		};
		Label detail = new Label()
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

			var editAction = new MenuItem { Text = Icons.EDIT };
			editAction.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));
			editAction.Clicked += OnEdit;

			var deleteAction = new MenuItem { Text = Icons.DELETE, IsDestructive = true };
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
			((View)Parent).Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<CalendarEvent>(ce, false)), true);
		}

		async void OnDelete(object sender, EventArgs e)
		{
			await LagerClient.CurrentLager.AddPacket(new DeleteCalendarEvent((CalendarEvent)((MenuItem)sender).CommandParameter));
		}
	}
}

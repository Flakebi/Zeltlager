using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	public partial class DayPage : ContentPage
	{
		public DayPage(Day day)
		{
			InitializeComponent();

			Padding = new Thickness(0, Device.OnPlatform(40, 40, 0), 0, 0);

			var dayNameLabel = new Label
			{
				Text = day.Date.ToString("dddd, dd.MM.")
			};
			var leftArrow = new Label
			{
				Text = "<-"
			};
			var rightArrow = new Label
			{
				Text = "->"
			};
			var header = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Children = { leftArrow, dayNameLabel, rightArrow }
			};

			var calendarList = new ListView();
			var customCell = new DataTemplate(typeof(CalendarEventCell));
			calendarList.ItemTemplate = customCell;
			calendarList.ItemsSource = day.Events;

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = { header, calendarList }
			};
		}
	}
}

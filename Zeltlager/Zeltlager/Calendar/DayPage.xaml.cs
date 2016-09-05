using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	public partial class DayPage : ContentPage
	{
		Button leftArrow, rightArrow;
		public Day Day { get; }

		public DayPage(Day day)
		{
			InitializeComponent();

			this.Day = day;

			Padding = new Thickness(5, 20, 5, 0);

			var dayNameLabel = new Label
			{
				Text = day.Date.ToString("dddd, dd.MM."),
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};

			leftArrow = new Button 
			{ 
				Text = "←", 
				FontAttributes = FontAttributes.Bold, 
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			leftArrow.Clicked += OnLeftButtonClicked;

			rightArrow = new Button
			{
				Text = "→",
				FontAttributes = FontAttributes.Bold,
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			rightArrow.Clicked += OnRightButtonClicked;

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
			calendarList.Header = header;
			calendarList.HorizontalOptions = LayoutOptions.CenterAndExpand;
			header.HorizontalOptions = LayoutOptions.FillAndExpand;
			Content = calendarList;

			//Content = new StackLayout
			//{
			//	VerticalOptions = LayoutOptions.FillAndExpand,
			//	Children = { header, calendarList }
			//};
			//removeNavButtons();
		}

		public void removeNavButtons() {
			//make nav buttons invisible at ends of calendar
			CarouselPage p = (CalendarPage)Parent;
			if (p.Children.IndexOf(this) == 0)
			{
				leftArrow.IsVisible = false;
			}
			else if (p.Children.IndexOf(this) == p.Children.Count-1)
			{
				rightArrow.IsVisible = false;
			}
		}

		private void OnLeftButtonClicked(object sender, EventArgs e)
		{
			CarouselPage p = (CalendarPage)Parent;
			p.CurrentPage = p.Children[p.Children.IndexOf(p.CurrentPage) - 1];
		}

		private void OnRightButtonClicked(object sender, EventArgs e)
		{
			CarouselPage p = (CalendarPage)Parent;
			p.CurrentPage = p.Children[p.Children.IndexOf(p.CurrentPage) + 1];
		}
	}
}

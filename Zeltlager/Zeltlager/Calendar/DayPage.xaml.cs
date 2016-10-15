using System;

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

			Day = day;

			Padding = new Thickness(0, 20, 15, 0);

			var dayNameLabel = new Label
			{
				Text = day.Date.ToString(Icons.WEEKDAYS[day.Date.DayOfWeek] + " dddd, dd.MM.yy"),
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};

			leftArrow = new Button
			{
				Margin = new Thickness(15, 0, 0, 0),
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
			// disable selection
			calendarList.ItemSelected += (sender, e) =>
			{
				((ListView)sender).SelectedItem = null;
			};

			header.HorizontalOptions = LayoutOptions.FillAndExpand;
			Content = calendarList;

			//Content = new StackLayout
			//{
			//	VerticalOptions = LayoutOptions.FillAndExpand,
			//	Children = { header, calendarList }
			//};
			//removeNavButtons();
		}

		public void removeNavButtons()
		{
			// Make nav buttons invisible at ends of calendar
			CarouselPage p = (CarouselPage)Parent;
			if (p.Children.IndexOf(this) == 0)
			{
				//leftArrow.IsVisible = false;
				leftArrow.Opacity = 0;
			} else if (p.Children.IndexOf(this) == p.Children.Count - 1)
			{
				//rightArrow.IsVisible = false;
				rightArrow.Opacity = 0;
			}
		}

		private void OnLeftButtonClicked(object sender, EventArgs e)
		{
			CarouselPage p = (CarouselPage)Parent;

			// Check for ends of the List
			if (p.Children.IndexOf(p.CurrentPage) > 0)
				p.CurrentPage = p.Children[p.Children.IndexOf(p.CurrentPage) - 1];
		}

		private void OnRightButtonClicked(object sender, EventArgs e)
		{
			CarouselPage p = (CarouselPage)Parent;

			if (p.Children.IndexOf(p.CurrentPage) < p.Children.Count - 1)
				p.CurrentPage = p.Children[p.Children.IndexOf(p.CurrentPage) + 1];
		}
	}
}

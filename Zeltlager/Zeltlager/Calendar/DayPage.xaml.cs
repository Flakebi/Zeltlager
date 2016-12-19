using System;

using Xamarin.Forms;

namespace Zeltlager.Calendar
{
	using Client;

	public partial class DayPage : ContentPage
	{
		Button leftArrow, rightArrow;
		public Day Day { get; }
		LagerClient lager;
		StackLayout dishwashers;
		bool editingDishwashers;
		Button editDishwasherButton;

		public DayPage(Day day, LagerClient lager)
		{
			this.lager = lager;
			InitializeComponent();

			Day = day;

			Padding = new Thickness(10);

			var dayNameLabel = new Label
			{
				Text = day.Date.ToString(Icons.WEEKDAYS[day.Date.DayOfWeek] + " dddd, dd.MM.yy"),
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};

			leftArrow = new Button
			{
				Image = Icons.ARROW_LEFT,
				FontAttributes = FontAttributes.Bold,
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button)),
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			leftArrow.Clicked += OnLeftButtonClicked;

			rightArrow = new Button
			{
				Image = Icons.ARROW_RIGHT,
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

			editDishwasherButton = new Button { Image = Icons.EDIT };
			editDishwasherButton.Clicked += OnEditDishwasherClicked;
			editDishwasherButton.HorizontalOptions = LayoutOptions.End;
			editingDishwashers = false;

			var label = new Label();
			if (Day.Dishwashers == null)
			{
				label.Text = "kein Spüldienst";
				label.TextColor = (Color)Application.Current.Resources["textColorSecondary"];
			}
			else
			{
				label.Text = "Spüldienst: " + Day.Dishwashers;
				label.TextColor = (Color)Application.Current.Resources["textColorSecondary"];
			}
			Label dishwasherLabel = label;
			dishwasherLabel.HorizontalOptions = LayoutOptions.CenterAndExpand;
			dishwasherLabel.VerticalOptions = LayoutOptions.CenterAndExpand;

			dishwashers = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Children = { dishwasherLabel, editDishwasherButton },
				Padding = new Thickness(10, 0, 0, 0)
			};

			var headerWithDishwashers = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Children = { header, dishwashers }
			};

			var calendarList = new ListView();
			var customCell = new DataTemplate(typeof(CalendarEventCell));
			calendarList.ItemTemplate = customCell;
			calendarList.ItemsSource = day.Events;
			calendarList.Header = headerWithDishwashers;
			calendarList.HorizontalOptions = LayoutOptions.CenterAndExpand;
			// disable selection
			calendarList.ItemSelected += (sender, e) =>
			{
				((ListView)sender).SelectedItem = null;
			};

			header.HorizontalOptions = LayoutOptions.FillAndExpand;
			Content = calendarList;
		}

		public void OnEditDishwasherClicked(object sender, EventArgs e)
		{
			editingDishwashers = !editingDishwashers;
			if (editingDishwashers)
			{
				editDishwasherButton.Image = Icons.SAVE;
				var picker = new Picker
				{
					Title = "Spüldienst wählen"
				};
				foreach (Tent tent in lager.Tents)
				{
					picker.Items.Add(tent.ToString());
				}
				// damit man auch wieder in den Startzustand ohne Spüldienst kommt
				picker.Items.Add("kein Spüldienst");
				picker.SelectedIndexChanged += (sendern, args) =>
				{
					if (picker.SelectedIndex == picker.Items.Count - 1)
					{
						Day.Dishwashers = null;
					}
					Day.Dishwashers = lager.GetTentFromDisplay(picker.Items[picker.SelectedIndex]);
				};

				if (Day.Dishwashers == null)
				{
					picker.SelectedIndex = picker.Items.IndexOf("kein Spüldienst");
				}
				else
				{
					picker.SelectedIndex = picker.Items.IndexOf(Day.Dishwashers.ToString());
				}

				picker.HorizontalOptions = LayoutOptions.CenterAndExpand;
				picker.VerticalOptions = LayoutOptions.CenterAndExpand;
				//picker.Style = (Style)Application.Current.Resources["BaseStyle"];
				dishwashers.Children.RemoveAt(0);
				dishwashers.Children.Insert(0, picker);
			}
			else
			{
				var label = new Label();
				if (Day.Dishwashers == null)
				{
					label.Text = "kein Spüldienst";
					label.TextColor = (Color)Application.Current.Resources["textColorSecondary"];
				}
				else
				{
					label.Text = "Spüldienst: " + Day.Dishwashers.ToString();
					label.TextColor = (Color)Application.Current.Resources["textColorSecondary"];
				}
				editDishwasherButton.Image = Icons.EDIT;
				dishwashers.Children.RemoveAt(0);
				label.HorizontalOptions = LayoutOptions.CenterAndExpand;
				label.VerticalOptions = LayoutOptions.CenterAndExpand;
				dishwashers.Children.Insert(0, label);
			}
		}

		public void removeNavButtons()
		{
			// Make nav buttons invisible at ends of calendar
			CarouselPage p = (CarouselPage)Parent;
			if (p.Children.IndexOf(this) == 0)
			{
				//leftArrow.IsVisible = false;
				leftArrow.Opacity = 0;
			}
			else if (p.Children.IndexOf(this) == p.Children.Count - 1)
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

using System;

using Xamarin.Forms;
using Zeltlager.UAM;
using System.Linq;

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
		public Command OnEdit { get; set; }
		public Command OnDelete { get; set; }

		public DayPage(Day day, LagerClient lager)
		{
			InitializeComponent();
			this.lager = lager;
			Day = day;

			//UpdateUI();
			Padding = new Thickness(10);
			//NavigationPage.SetBackButtonTitle(this, "");
		}

		void OnEditClicked(IListCalendarEvent ilce)
		{
			if (ilce is ReferenceCalendarEvent)
			{
				Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<ExRefCalendarEvent, PlannedCalendarEvent>
				           (new ExRefCalendarEvent((ReferenceCalendarEvent)ilce), true, lager)),true);
			} else if (ilce is CalendarEvent)
			{
				Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<CalendarEvent, PlannedCalendarEvent>
						   ((CalendarEvent) ilce, true, lager)), true);
			}
		}

		void OnDeleteClicked(IListCalendarEvent ilce)
		{
			ilce.IsVisible = false;
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

		public void RemoveNavButtons()
		{
			// Make nav buttons invisible at ends of calendar
			CarouselPage p = (CarouselPage)Parent;
			if (p.Children.First() == this)
			{
				//leftArrow.IsVisible = false;
				leftArrow.Opacity = 0;
			}
			if (p.Children.Last() == this)
			{
				//rightArrow.IsVisible = false;
				rightArrow.Opacity = 0;
			}
		}

		void OnLeftButtonClicked(object sender, EventArgs e)
		{
			CarouselPage p = (CarouselPage)Parent;

			// Check for ends of the List
			if (p.Children.IndexOf(p.CurrentPage) > 0)
				p.CurrentPage = p.Children[p.Children.IndexOf(p.CurrentPage) - 1];
		}

		void OnRightButtonClicked(object sender, EventArgs e)
		{
			CarouselPage p = (CarouselPage)Parent;

			if (p.Children.IndexOf(p.CurrentPage) < p.Children.Count - 1)
				p.CurrentPage = p.Children[p.Children.IndexOf(p.CurrentPage) + 1];
		}

		public void UpdateUI()
		{
			var dayNameLabel = new Label
			{
				Text = Day.Date.ToString(Icons.WEEKDAYS[Day.Date.DayOfWeek] + " dddd, dd.MM.yy"),
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

			var dataTemplate = new DataTemplate(typeof(GeneralCalendarEventCell));
			OnEdit = new Command(sender => OnEditClicked((IListCalendarEvent)sender));
			OnDelete = new Command(sender => OnDeleteClicked((IListCalendarEvent)sender));

			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandProperty, new Binding(nameof(OnEdit), source: this));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandProperty, new Binding(nameof(OnDelete), source: this));

			calendarList.ItemTemplate = dataTemplate;
			calendarList.ItemsSource = Day.Events;//.Where(x => x.IsShown);
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

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
			RemoveNavButtons();
		}

		protected override void OnParentSet()
		{
			base.OnParentSet();
			if (Parent != null)
			{
				OnAppearing();
			}
		}
	}
}

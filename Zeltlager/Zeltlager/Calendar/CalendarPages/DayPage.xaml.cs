using System;

using Xamarin.Forms;
using Zeltlager.UAM;
using System.Linq;
using System.Threading.Tasks;

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
				Navigation.PushAsync(new UniversalAddModifyPage<ExRefCalendarEvent, PlannedCalendarEvent>
						   (new ExRefCalendarEvent((ReferenceCalendarEvent)ilce), false, lager));
			} else if (ilce is CalendarEvent)
			{
				Navigation.PushAsync(new UniversalAddModifyPage<CalendarEvent, PlannedCalendarEvent>
						   ((CalendarEvent)ilce, false, lager));
			}
		}

		async Task OnDeleteClicked(IListCalendarEvent ilce)
		{
			await ilce.Delete(lager);
			OnAppearing();
		}

		public async void OnEditDishwasherClicked(object sender, EventArgs e)
		{
			editingDishwashers = !editingDishwashers;
			if (editingDishwashers)
			{
				editDishwasherButton.Image = Icons.SAVE;
				var picker = new Picker
				{
					Title = "Spüldienst wählen"
				};
				foreach (Tent tent in lager.VisibleTents)
				{
					picker.Items.Add(tent.ToString());
				}
				// damit man auch wieder in den Startzustand ohne Spüldienst kommt
				picker.Items.Add("kein Spüldienst");
				picker.SelectedIndexChanged += (sendern, args) =>
				{
					if (picker.Items[picker.SelectedIndex] == "kein Spüldienst")
					{
						Day.Dishwashers = null;
					}
					Tent dishwasherTent = lager.GetTentFromDisplay(picker.Items[picker.SelectedIndex]);
					Day.Dishwashers = dishwasherTent;
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
				// display dishwasher state
				var label = new Label();
				if (Day.Dishwashers == null)
				{
					await Day.CreateDishwasherPacket(null, lager);
					label.Text = "kein Spüldienst";
					label.TextColor = (Color)Application.Current.Resources["textColorButton"];
				}
				else
				{
					await Day.CreateDishwasherPacket(Day.Dishwashers, lager);
					label.Text = "Spüldienst: " + Day.Dishwashers;
					label.TextColor = (Color)Application.Current.Resources["textColorButton"];
				}
				editDishwasherButton.Image = Icons.EDIT;
				dishwashers.Children.RemoveAt(0);
				label.HorizontalOptions = LayoutOptions.CenterAndExpand;
				label.VerticalOptions = LayoutOptions.CenterAndExpand;
				dishwashers.Children.Insert(0, label);
			}
		}

		public void UpdateNavButtons()
		{
			// Make nav buttons invisible at ends of calendar
			CarouselPage p = (CarouselPage)Parent;
			leftArrow.Opacity = p.Children.First() == this ? 0 : 1;
			rightArrow.Opacity = p.Children.Last() == this ? 0 : 1;
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

			#region Dishwashers
			editDishwasherButton = new Button { Image = Icons.EDIT };
			editDishwasherButton.Clicked += OnEditDishwasherClicked;
			editDishwasherButton.HorizontalOptions = LayoutOptions.End;
			editingDishwashers = false;

			var label = new Label();
			if (Day.Dishwashers == null)
			{
				label.Text = "kein Spüldienst";
				label.TextColor = (Color)Application.Current.Resources["textColorButton"];
			}
			else
			{
				label.Text = "Spüldienst: " + Day.Dishwashers;
				label.TextColor = (Color)Application.Current.Resources["textColorButton"];
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
			#endregion 

			var calendarList = new ListView();

			var dataTemplate = new DataTemplate(typeof(GeneralCalendarEventCell));
			OnEdit = new Command(sender => OnEditClicked((IListCalendarEvent)sender));
			OnDelete = new Command(async sender => await OnDeleteClicked((IListCalendarEvent)sender));

			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnEditCommandProperty, new Binding(nameof(OnEdit), source: this));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandParameterProperty, new Binding("."));
			dataTemplate.SetBinding(GeneralCalendarEventCell.OnDeleteCommandProperty, new Binding(nameof(OnDelete), source: this));

			calendarList.ItemTemplate = dataTemplate;
			calendarList.ItemsSource = Day.Events.Where(ce => ce.IsVisible);
			calendarList.Header = headerWithDishwashers;
			calendarList.HorizontalOptions = LayoutOptions.CenterAndExpand;
			// disable selection
			calendarList.ItemSelected += (sender, e) =>
			{
				OnEditClicked((IListCalendarEvent)((ListView)sender).SelectedItem);
			};

			header.HorizontalOptions = LayoutOptions.FillAndExpand;
			Content = calendarList;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
			UpdateNavButtons();
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

using System;
using Xamarin.Forms;
using Zeltlager.UAM;

namespace Zeltlager.Competition
{
	public partial class StationPage : ContentPage
	{
		Station station;

		public StationPage(Station station)
		{
			InitializeComponent();
			this.station = station;

			CreateUI();
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushModalAsync(new NavigationPage(new UniversalAddModifyPage<CompetitionResult, CompetitionResult>(new CompetitionResult(null, station, null), true, station.GetLagerClient())), true);
		}

		void CreateUI()
		{
			StackLayout vsl = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				VerticalOptions = LayoutOptions.StartAndExpand,
				Padding = new Thickness(10),
			};

			Button addResult = new Button
			{
				Style = (Style)Application.Current.Resources["DarkButtonStyle"],
				Text = "Ergebnis hinzufügen",
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			addResult.Clicked += OnAddButtonClicked;

			ListView participantResults = new ListView
			{
				ItemTemplate = new DataTemplate(typeof(ParticipantResultCell)),
				BindingContext = station.Ranking.Results,
				ItemsSource = station.Ranking.Results,
			};
			participantResults.ItemSelected += (sender, e) => { participantResults.SelectedItem = null; };
			StackLayout prh = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
			};
			prh.Children.Add(new ContentView
			{
				HorizontalOptions = LayoutOptions.StartAndExpand,
			});// so icons get moved to the right

			prh.Children.Add(new Button
			{
				Image = Icons.ADD,
				HorizontalOptions = LayoutOptions.Center,
			});
			prh.Children.Add(new Button
			{
				Image = Icons.DELETE,
				HorizontalOptions = LayoutOptions.End,
			});
			participantResults.Header = prh;

			StackLayout hsl = new StackLayout { Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.End };
			Button increasing = new Button
			{
				Text = "aufsteigend ranken",
				Style = (Style)Application.Current.Resources["DarkButtonStyle"],
				HorizontalOptions = LayoutOptions.StartAndExpand,
				Margin = new Thickness(5),
			};
			increasing.Clicked += (sender, e) =>
			{
				station.Ranking.Rank(true);
				CreateUI();
			};
			Button decreasing = new Button
			{
				Text = "absteigend ranken",
				Style = (Style)Application.Current.Resources["DarkButtonStyle"],
				HorizontalOptions = LayoutOptions.EndAndExpand,
				Margin = new Thickness(5),
			};
			decreasing.Clicked += (sender, e) =>
			{
				station.Ranking.Rank(false);
				CreateUI();
			};
			hsl.Children.Add(increasing);
			hsl.Children.Add(decreasing);

			vsl.Children.Add(addResult);
			vsl.Children.Add(participantResults);
			vsl.Children.Add(hsl);

			Content = vsl;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			CreateUI();
		}
	}
}

using System;
using Xamarin.Forms;

namespace Zeltlager.Competition
{
	public partial class StationPage : ContentPage
	{
		Station station;

		public StationPage(Station station)
		{
			InitializeComponent();
			this.station = station;

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
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			addResult.Clicked += OnAddButtonClicked;

			ListView participantResults = new ListView
			{
				ItemTemplate = new DataTemplate(typeof(ParticipantResultCell)),
				BindingContext = station.Ranking.Results,
				ItemsSource = station.Ranking.Results,
			};

			StackLayout hsl = new StackLayout { Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.End };
			Button increasing = new Button
			{
				Text = "aufsteigend sortieren",
				Style = (Style)Application.Current.Resources["DarkButtonStyle"],
			};
			Button decreasing = new Button
			{
				Text = "absteigend sortieren",
				Style = (Style)Application.Current.Resources["DarkButtonStyle"],
			};
			hsl.Children.Add(increasing);
			hsl.Children.Add(decreasing);

			vsl.Children.Add(addResult);
			vsl.Children.Add(participantResults);
			vsl.Children.Add(hsl);

			Content = vsl;
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			// TODO call uam on competition result
		}
	}
}

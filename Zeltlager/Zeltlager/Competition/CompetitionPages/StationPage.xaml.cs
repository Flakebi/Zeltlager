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

			StackLayout vsl = new StackLayout { Orientation = StackOrientation.Vertical, VerticalOptions = LayoutOptions.StartAndExpand };

			Button addResult = new Button
			{
				Style = (Style)Application.Current.Resources["DarkButtonStyle"],
				Text = "Ergebnis hinzufügen",
				HorizontalOptions = LayoutOptions.CenterAndExpand,
			};

			ListView participantResults = new ListView
			{
				ItemTemplate = new DataTemplate(typeof(ParticipantResultCell)),
				BindingContext = station.Ranking.Results,
				ItemsSource = station.Ranking.Results,
			};

			StackLayout hsl = new StackLayout { Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.End };
			Button increasing = new Button
			{
				Text = "aufsteigend sortieren"
			};
			Button decreasing = new Button
			{
				Text = "absteigend sortieren"
			};
			hsl.Children.Add(increasing);
			hsl.Children.Add(decreasing);

			vsl.Children.Add(addResult);
			vsl.Children.Add(participantResults);
			vsl.Children.Add(hsl);

			Content = vsl;
		}
	}
}

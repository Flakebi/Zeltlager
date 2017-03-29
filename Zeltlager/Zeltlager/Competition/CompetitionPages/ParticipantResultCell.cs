using Xamarin.Forms;

namespace Zeltlager.Competition
{
	public class ParticipantResultCell : ViewCell
	{
		public Label Points { get; private set; }
		public Label Place { get; private set; }

		public ParticipantResultCell()
		{
			StackLayout hsl = new StackLayout { Orientation = StackOrientation.Horizontal };

			Label participant = new Label
			{
				HorizontalOptions = LayoutOptions.StartAndExpand,
			};
			Points = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 48,
			};
			Place = new Label
			{
				HorizontalOptions = LayoutOptions.End,
				WidthRequest = 48,
			};

			hsl.Children.Add(participant);
			hsl.Children.Add(Points);
			hsl.Children.Add(Place);

			participant.SetBinding(Label.TextProperty, "Participant.Name");
			Points.SetBinding(Label.TextProperty, "PointsString");
			Place.SetBinding(Label.TextProperty, "PlaceString");

			View = hsl;
		}
	}
}

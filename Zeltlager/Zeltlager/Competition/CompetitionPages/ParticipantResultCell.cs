using System;
using Xamarin.Forms;

namespace Zeltlager.Competition
{
	public class ParticipantResultCell : ViewCell
	{
		public ParticipantResultCell()
		{
			StackLayout hsl = new StackLayout { Orientation = StackOrientation.Horizontal };

			Label participant = new Label
			{
				HorizontalOptions = LayoutOptions.StartAndExpand,
			};
			Label points = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
			};
			Button scrap = new Button
			{
				Image = Icons.PODIUM,
			};
			hsl.Children.Add(scrap);
			Label place = new Label
			{
				HorizontalOptions = LayoutOptions.End,
				WidthRequest = scrap.Width,
			};

			hsl.Children.Add(participant);
			hsl.Children.Add(points);
			hsl.Children.Add(place);
			hsl.Children.Remove(scrap);

			participant.SetBinding(Label.TextProperty, "Participant.Name");
			points.SetBinding(Label.TextProperty, "PointsString");
			place.SetBinding(Label.TextProperty, "PlaceString");

			View = hsl;
		}
	}
}


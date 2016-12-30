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
			Label place = new Label
			{
				HorizontalOptions = LayoutOptions.End,
				WidthRequest = 35,
			};

			hsl.Children.Add(participant);
			hsl.Children.Add(points);
			hsl.Children.Add(place);

			participant.SetBinding(Label.TextProperty, "Participant.Name");
			points.SetBinding(Label.TextProperty, "PointsString");
			place.SetBinding(Label.TextProperty, "PlaceString");

			View = hsl;
		}
	}
}


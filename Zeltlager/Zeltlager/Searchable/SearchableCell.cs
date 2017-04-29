using Xamarin.Forms;

namespace Zeltlager
{
	public class SearchableCell : ActionCell
	{
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(SearchableCell), null);
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set
			{
				SetValue(TextProperty, value);
				OnPropertyChanged();
			}
		}
		public static readonly BindableProperty DetailProperty = BindableProperty.Create(nameof(Detail), typeof(string), typeof(SearchableCell), null);
		public string Detail
		{
			get { return (string)GetValue(DetailProperty); }
			set
			{
				SetValue(DetailProperty, value);
				OnPropertyChanged();
			}
		}

		Label TextLabel;
		Label DetailLabel;

		public SearchableCell()
		{
			StackLayout hsl = new StackLayout
			{
				Spacing = 2,
				Margin = 5,
			};

			TextLabel = new Label
			{
				TextColor = (Color)Application.Current.Resources["textColorStandard"],
				LineBreakMode = LineBreakMode.HeadTruncation,
			};
			DetailLabel = new Label
			{
				FontSize = TextLabel.FontSize - 3,
				TextColor = (Color) Application.Current.Resources["detailColor"],
				LineBreakMode = LineBreakMode.HeadTruncation,
			};

			hsl.Children.Add(TextLabel);
			hsl.Children.Add(DetailLabel);

			TextLabel.SetBinding(Label.TextProperty, new Binding(nameof(Text), source: this));
			DetailLabel.SetBinding(Label.TextProperty, new Binding(nameof(Detail), source: this));

			View = hsl;
		}
	}
}

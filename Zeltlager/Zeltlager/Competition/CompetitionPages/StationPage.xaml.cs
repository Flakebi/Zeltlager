using Xamarin.Forms;

namespace Zeltlager.Competition
{
	public partial class StationPage : ContentPage
	{
		Station station;
		RankingView rankingView;

		public StationPage(Station station)
		{
			InitializeComponent();
			this.station = station;

			// One time UI setup
			BindingContext = station;

			rankingView = new RankingView(station.GetLagerClient(), station);
			Content = rankingView;
			
			NavigationPage.SetBackButtonTitle(this, "");
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			rankingView.UpdateUI();
		}
	}
}

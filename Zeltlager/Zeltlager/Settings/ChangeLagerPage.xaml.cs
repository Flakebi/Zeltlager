using System.Linq;

using Xamarin.Forms;

namespace Zeltlager.Settings
{
	using Client;

	public partial class ChangeLagerPage : ContentPage
	{
		// current (old) lager
		LagerClient lager;

		public ChangeLagerPage(LagerClient lager, LagerClientManager manager)
		{
			InitializeComponent();
			this.lager = lager;
			NavigationPage.SetBackButtonTitle(this, "");
			Content = new SearchableListView<LagerClient>(manager.Lagers.Values.Cast<LagerClient>().ToList(),
			                                              null, null, OnLagerClicked);
		}

		void OnLagerClicked(LagerClient newlager)
		{
			((App)Application.Current).ChangeLager(lager, newlager);
		}
	}
}

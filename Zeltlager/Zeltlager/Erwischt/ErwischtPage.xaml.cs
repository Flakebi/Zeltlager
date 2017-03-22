using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Zeltlager.Client;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Erwischt
{
	public partial class ErwischtPage : ContentPage
	{
		Erwischt game;
		LagerClient lager;

		public ErwischtPage(Erwischt game, LagerClient lager)
		{
			InitializeComponent();
			this.game = game;
			this.lager = lager;
			UpdateUI();
		}

		void UpdateUI()
		{
			Content = new SearchableListView<ErwischtMember>(game.VisibleParticipants,
															 OnEditClicked, OnDeleteClicked, OnErwischtMemberClicked);
		}

		void OnErwischtMemberClicked(ErwischtMember member)
		{
			Navigation.PushModalAsync(new ErwischtMemberDetailPage(member), true);
		}

		void OnEditClicked(ErwischtMember member) { }

		async Task OnDeleteClicked(ErwischtMember member)
		{
			member.IsVisible = false;
			// TODO Packages
		}

		void OnAddButtonClicked(object sender, EventArgs e)
		{
			// TODO UAM for erwischt game, update current game in erwischt handler -> add package!!
		}

		void OnChangeGameButtonClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(new ChangeErwischtGamePage(lager.ErwischtHandler), true);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			UpdateUI();
		}
	}
}

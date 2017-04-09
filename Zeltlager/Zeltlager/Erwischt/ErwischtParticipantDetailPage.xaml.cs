using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Threading.Tasks;

namespace Zeltlager.Erwischt
{
	public partial class ErwischtParticipantDetailPage : ContentPage
	{
		public ErwischtParticipant ErwischtParticipant { get; set; }

		public ErwischtParticipantDetailPage(ErwischtParticipant member)
		{
			InitializeComponent();
			ErwischtParticipant = member;
			BindingContext = ErwischtParticipant;
			UpdateUI();
			NavigationPage.SetBackButtonTitle(this, "");
		}

		async void OnCatchTargetClicked(object sender, EventArgs e)
		{
			await ErwischtParticipant.Target.Catch();
			UpdateUI();
		}

		async void OnReviveClicked(object sender, EventArgs e)
		{
			await ErwischtParticipant.Revive();
			UpdateUI();
		}

		void UpdateUI()
		{
			targetName.Text = ErwischtParticipant.SearchableDetail;
			reviveButton.IsVisible = !ErwischtParticipant.IsAlive;

			BindingContext = null;
			BindingContext = ErwischtParticipant;
		}
	}
}

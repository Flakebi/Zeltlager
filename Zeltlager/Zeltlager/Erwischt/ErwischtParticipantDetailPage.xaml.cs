using System;

using Xamarin.Forms;

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
			if (string.IsNullOrEmpty(targetName.Text))
				targetName.Text = "→ " + ErwischtParticipant.Target.Member.Display;
			reviveButton.IsVisible = !ErwischtParticipant.IsAlive;

			BindingContext = null;
			BindingContext = ErwischtParticipant;
		}
	}
}

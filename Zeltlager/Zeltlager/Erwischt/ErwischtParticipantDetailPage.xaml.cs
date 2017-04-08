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
			if (ErwischtParticipant.IsAlive)
			{
				targetName.Text = ErwischtParticipant.Target.Member.Name;
				reviveButton.IsVisible = false;
			}
			else
			{
				targetName.Text = "Erwischt!";
				reviveButton.IsVisible = true;
			}
			BindingContext = null;
			BindingContext = ErwischtParticipant;
		}
	}
}

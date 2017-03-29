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
			NavigationPage.SetBackButtonTitle(this, "");
		}

		async void OnCatchTargetClicked(object sender, EventArgs e)
		{
			await ErwischtParticipant.Target.Catch();
			OnPropertyChanged(nameof(ErwischtParticipant));
		}

		async void OnReviveClicked(object sender, EventArgs e)
		{
			await ErwischtParticipant.Revive();
			OnPropertyChanged(nameof(ErwischtParticipant));
		}
	}
}

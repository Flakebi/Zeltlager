using System;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Threading.Tasks;

namespace Zeltlager.Erwischt
{
	public partial class ErwischtParticipantDetailPage : ContentPage
	{
		public ErwischtParticipant Member { get; set; }

		public ErwischtParticipantDetailPage(ErwischtParticipant member)
		{
			InitializeComponent();
			Member = member;
			BindingContext = Member;
		}

		async Task OnCatchTargetClicked(object sender, EventArgs e)
		{
			await Member.Target.Catch();
		}

		async Task OnReviveClicked(object sender, EventArgs e)
		{
			await Member.Revive();
		}
	}
}

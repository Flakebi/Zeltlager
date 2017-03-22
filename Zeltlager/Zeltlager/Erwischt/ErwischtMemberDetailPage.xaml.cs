using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Erwischt
{
	public partial class ErwischtMemberDetailPage : ContentPage
	{
		public ErwischtMember Member { get; set; }

		public ErwischtMemberDetailPage(ErwischtMember member)
		{
			InitializeComponent();
			Member = member;
			BindingContext = Member;
		}

		public void OnCatchTargetClicked(object sender, EventArgs e)
		{
			Member.Target.IsAlive = false;
			// TODO packages
		}
	}
}

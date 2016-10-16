using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Competition
{
	public partial class CompetitionPage : ContentPage
	{
		Competition competition;

		public CompetitionPage(Competition competition)
		{
			InitializeComponent();
			this.competition = competition;
		}
	}
}

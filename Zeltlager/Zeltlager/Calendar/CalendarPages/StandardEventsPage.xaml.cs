using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Zeltlager.Calendar.CalendarPages
{
	public partial class StandardEventsPage : ContentPage
	{
		Calendar calendar;

		public StandardEventsPage(Calendar c)
		{
			InitializeComponent();
			calendar = c;
			BindingContext = calendar;
		}

		void OnAddClicked()
		{
			
		}
	}
}

using System;

using Xamarin.Forms;

namespace Zeltlager
{
	public class TentDetailPage : ContentPage
	{
		public TentDetailPage()
		{
			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Hello ContentPage" }
				}
			};
		}
	}
}


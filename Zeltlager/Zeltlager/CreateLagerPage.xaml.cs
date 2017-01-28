using System;
using Xamarin.Forms;

namespace Zeltlager
{
	public partial class CreateLagerPage : ContentPage
	{
		public string Name { get; set; }
		public string Password { get; set; }

		public CreateLagerPage()
		{
			Name = "";
			Password = "";

			InitializeComponent();
			BindingContext = this;
			NavigationPage.SetBackButtonTitle(this, "");
		}

		async void OnCreateClicked(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(Name))
			{
				await DisplayAlert(Title, "Bitte gebe einen Namen ein", "Ok");
				return;
			}

			if (string.IsNullOrEmpty(Password))
			{
				await DisplayAlert(Title, "Bitte gebe ein Passwort ein", "Ok");
				return;
			}

			await ((App)Application.Current).CreateLager(Name, Password);
		}
	}
}

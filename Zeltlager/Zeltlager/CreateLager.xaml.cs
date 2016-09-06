using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Zeltlager
{
	public partial class CreateLager : ContentPage
	{
		App app;

		public string Name { get; set; }
		public string Password { get; set; }

		public CreateLager(App app)
		{
			this.app = app;
			Name = "";
			Password = "";

			InitializeComponent();
			BindingContext = this;
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

			await app.CreateLager(Name, Password);
		}
	}
}

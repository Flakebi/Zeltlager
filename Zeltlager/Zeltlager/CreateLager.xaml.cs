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

			InitializeComponent();
			BindingContext = this;
		}

		async void OnCreateClicked(object sender, EventArgs e)
		{
			await app.CreateLager(Name, Password);
		}
	}
}

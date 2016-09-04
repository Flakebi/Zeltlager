using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Zeltlager
{
	public partial class LoadingScreen : ContentPage
	{
		string status = "Laden";
		public string Status
		{
			get
			{
				return status;
			}

			set
			{
				status = value;
				OnPropertyChanged("Status");
			}
		}

		public LoadingScreen()
		{
			InitializeComponent();
			BindingContext = this;
		}
	}
}

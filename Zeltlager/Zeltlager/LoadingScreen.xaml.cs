using Xamarin.Forms;

namespace Zeltlager
{
	public partial class LoadingScreen : ContentPage
	{
		string status = "Laden";
		public string Status
		{
			get { return status; }

			set
			{
				status = value;
				OnPropertyChanged(nameof(Status));
			}
		}

		public LoadingScreen()
		{
			InitializeComponent();
			Padding = new Thickness(10);
			BindingContext = this;
		}
	}
}

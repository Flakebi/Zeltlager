using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Zeltlager
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			Lager.IsClient = true;
			Lager.IoProvider = new ClientIoProvider();
			// Set the current lager
			//TODO Don't set default Lager
			Lager.CurrentLager = new Lager(0, "Default", "pass");
			Lager.CurrentLager.Save(Lager.IoProvider);
			MainPage = new NavigationPage(new MainPage());
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}

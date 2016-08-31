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
			//TODO
			Lager.CurrentLager = new Lager("Default");
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

using System;
using System.Threading.Tasks;
using System.Linq;

using Xamarin.Forms;

namespace Zeltlager
{
	using Client;
	using Network;

	public partial class App : Application
	{
		static readonly string[] INIT_STATUS =
		{
			"Lagerschlüssel erstellen",
			"Lagerzertifikat erstellen",
			"Persönliches Zertifikat erstellen",
			"Lager speichern"
		};

		LoadingScreen loadingScreen;
		LagerClientManager manager;

		public App()
		{
			InitializeComponent();

			LagerManager.IsClient = true;

			manager = new LagerClientManager(new IoProvider());
			manager.NetworkClient = new TcpNetworkClient();

			loadingScreen = new LoadingScreen();
			MainPage = new NavigationPage(loadingScreen);
		}

		protected async override void OnStart()
		{
			// Load the log
			try
			{
				loadingScreen.Status = "Log laden";
				await LagerManager.Log.Load();
			} catch (Exception e)
			{
				// Log the exception
				await LagerManager.Log.Exception("Load log", e);
				await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
			}
			// Load settings
			try
			{
				loadingScreen.Status = "Einstellungen laden";
				await manager.Load();
			} catch (Exception e)
			{
				// Log the exception
				await LagerManager.Log.Exception("Load lagers", e);
				await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
			}

			bool loadedLager = false;
			LagerClient lager = null;
			int lagerId = manager.Settings.LastLager;
			if (manager.Lagers.ContainsKey(lagerId))
			{
				// Load lager
				try
				{
					loadingScreen.Status = "Lager laden";
					lager = (LagerClient)manager.Lagers[lagerId];
					if (!await lager.LoadBundles())
						await MainPage.DisplayAlert(loadingScreen.Status, "Beim Laden der Lagerdateien sind Fehler aufgetreten", "Ok");
					if (!await lager.ApplyHistory())
						await MainPage.DisplayAlert(loadingScreen.Status, "Beim Laden des Lagers sind Fehler aufgetreten", "Ok");
					loadedLager = true;
				} catch (Exception e)
				{
					// Log the exception
					await LagerManager.Log.Exception("Load lager", e);
					await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
				}
			}
			if (loadedLager)
				// Go to the main page
				MainPage = new NavigationPage(new MainPage(lager));
			else
			{
				// Create lager
				loadingScreen.Status = "Lager erstellen";
				//TODO Go to the settings screen here so the user can set the server and download a lager
				MainPage = new NavigationPage(new CreateLagerPage());
			}
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}

		void DisplayStatus(LagerClient.InitStatus status) => loadingScreen.Status = INIT_STATUS[(int)status];

		public async Task CreateLager(string name, string password)
		{
			MainPage = new NavigationPage(loadingScreen);
			try
			{
				var lager = await manager.CreateLager(name, password, DisplayStatus);
				// fill with some test data
				await lager.CreateTestData();




				// Go to the main page
				MainPage = new NavigationPage(new MainPage(lager));
			} catch (Exception e)
			{
				// Log the exception
				await LagerManager.Log.Exception("Creating lager", e);
				await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
			}
		}
	}
}

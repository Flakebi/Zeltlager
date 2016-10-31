using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;

namespace Zeltlager
{
	using Client;

	public partial class App : Application
	{
		static readonly string[] INIT_STATUS =
		{
			"Lagerschlüssel erstellen",
			"Lagerzertifikat erstellen",
			"Persönliches Zertifikat erstellen",
			"Fertig"
		};

		LoadingScreen loadingScreen;
        ClientLagerManager manager;

		public App()
		{
			InitializeComponent();

            LagerManager.IsClient = true;

            manager = new ClientLagerManager(new IoProvider());

			loadingScreen = new LoadingScreen();
			MainPage = new NavigationPage(loadingScreen);
		}

		protected async override void OnStart()
		{
			// Load settings
			try
			{
				loadingScreen.Status = "Einstellungen laden";
                await manager.Load();
				await LagerManager.Log.Load();
			} catch (Exception e)
			{
				// Log the exception
				await LagerManager.Log.Exception("App", e);
				await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
			}

			bool loadedLager = false;
			LagerClient lager = null;
            if (manager.Lagers.Any())
			{
				// Load lager
				try
				{
					loadingScreen.Status = "Lager laden";
                    int lagerId = manager.Settings.LastLager;
                    lager = (LagerClient)manager.Lagers[lagerId];
					if (!await lager.LoadBundles())
						await MainPage.DisplayAlert(loadingScreen.Status, "Beim Laden des Lagers sind Fehler aufgetreten", "Ok");
					loadedLager = true;
				} catch (Exception e)
				{
					// Log the exception
					await LagerManager.Log.Exception("App", e);
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
				MainPage = new NavigationPage(new CreateLager(this));
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

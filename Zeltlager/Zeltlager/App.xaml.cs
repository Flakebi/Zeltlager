using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Zeltlager.Settings;

namespace Zeltlager
{
	using Client;
	using Network;

	public partial class App : Application
	{
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

			LagerClient lager = null;
			int lagerId = manager.Settings.LastLager;
			if (manager.Lagers.ContainsKey(lagerId))
			{
				lager = (LagerClient)manager.Lagers[lagerId];
				ChangeLager(null, lager);
			}
			else
			{
				// Create lager
				loadingScreen.Status = "Lager erstellen";
				MainPage = new NavigationPage(new ManageLagerPage(null, manager));
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

		public async Task CreateLager(string name, string password)
		{
			MainPage = new NavigationPage(loadingScreen);
			try
			{
				var lager = await manager.CreateLager(name, password, status => loadingScreen.Status = status.GetMessage());

				// Go to the main page
				MainPage = new NavigationPage(new MainPage(lager));
			}
			catch (Exception e)
			{
				// Log the exception
				await LagerManager.Log.Exception("Creating lager", e);
				await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
			}
		}

		// call this if you want to change the current lager
		async public void ChangeLager(LagerClient oldLager, LagerClient newlager)
		{
			// unloads lager if it is not null
			oldLager?.Unload();
			
			if (!await newlager.LoadBundles())
			{
				await MainPage.DisplayAlert("Achtung!", "Beim Laden der Datenblöcke ist ein Fehler aufgetreten.", "Ok");
			}
			if (!await newlager.ApplyHistory())
			{
				await MainPage.DisplayAlert("Achtung!", "Beim Anwenden der Datenpakete ist ein Fehler aufgetreten.", "Ok");
			}
			manager.Settings.LastLager = newlager.Id;
			await manager.Settings.Save(manager.IoProvider);
			MainPage = new NavigationPage(new MainPage(newlager));
		}
		
	}
}

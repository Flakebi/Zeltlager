using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Zeltlager
{
	using Client;

	public partial class App : Application
	{
		static readonly string[] INIT_STATUS = new string[]
		{
			"Lagerschlüssel erstellen",
			"Lagerzertifikat erstellen",
			"Persönliches Zertifikat erstellen",
			"Fertig"
		};

		LoadingScreen loadingScreen;

		public App()
		{
			InitializeComponent();

			LagerBase.IsClient = true;
			LagerBase.IoProvider = new Client.IoProvider();
			LagerClient.ClientGlobalSettings = new Client.GlobalSettings();

			/*lager.Init();
			Tent tent = new Tent(0, "Regenbogenforellen", new List<Member>());
			DataPacket packet = new AddTentPacket(tent);
			lager.Collaborators.First().AddPacket(packet);
			var member = new Member(0, "Caro", tent, true);
			packet = new AddMemberPacket(member);
			lager.Collaborators.First().AddPacket(packet);*/

			loadingScreen = new LoadingScreen();
			MainPage = new NavigationPage(loadingScreen);
		}

		protected async override void OnStart()
		{
			// Load settings
			try
			{
				loadingScreen.Status = "Einstellungen laden";
				await LagerClient.ClientGlobalSettings.Load();
				await LagerBase.Log.Load();
			} catch (Exception e)
			{
				// Log the exception
				await LagerBase.Log.Exception("App", e);
				await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
			}

			bool loadedLager = false;
			if (LagerClient.ClientGlobalSettings.Lagers.Count > 0)
			{
				// Load lager
				try
				{
					loadingScreen.Status = "Lager laden";
					byte lagerId = LagerClient.ClientGlobalSettings.LastLager;
					var lagerData = LagerClient.ClientGlobalSettings.Lagers[lagerId];
					LagerClient.CurrentLager = new LagerClient(lagerId, lagerData.Item1, lagerData.Item2);
					if (!await LagerClient.CurrentLager.Load())
						await MainPage.DisplayAlert(loadingScreen.Status, "Beim Laden des Lagers sind Fehler aufgetreten", "Ok");
					loadedLager = true;
				} catch (Exception e)
				{
					// Log the exception
					await LagerBase.Log.Exception("App", e);
					await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
				}
			}
			if (loadedLager)
				// Go to the main page
				MainPage = new NavigationPage(new MainPage());
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
				LagerClient.CurrentLager = new LagerClient((byte)LagerClient.ClientGlobalSettings.Lagers.Count, name, password);
				await LagerClient.CurrentLager.Init(DisplayStatus);
				loadingScreen.Status = "Lager speichern";
				await LagerClient.CurrentLager.Save();

				// Add lager to settings
				loadingScreen.Status = "Einstellungen speichern";
				LagerClient.ClientGlobalSettings.LastLager = (byte)LagerClient.ClientGlobalSettings.Lagers.Count;
				LagerClient.ClientGlobalSettings.Lagers.Add(new Tuple<string, string>(name, password));
				await LagerClient.ClientGlobalSettings.Save();

				// Go to the main page
				MainPage = new NavigationPage(new MainPage());
			} catch (Exception e)
			{
				// Log the exception
				await LagerBase.Log.Exception("App", e);
				await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
			}
		}
	}
}

using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Zeltlager
{
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

			Lager.IsClient = true;
			Lager.IoProvider = new Client.IoProvider();
			Lager.ClientGlobalSettings = new Client.GlobalSettings();

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
				await Lager.ClientGlobalSettings.Load();
				await Lager.Log.Load();
			}
			catch (Exception e)
			{
				// Log the exception
				await Lager.Log.Exception("App", e);
				await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
			}

			if (Lager.ClientGlobalSettings.Lagers.Count > 0)
			{
				// Load lager
				try
				{
					loadingScreen.Status = "Lager laden";
					byte lagerId = Lager.ClientGlobalSettings.LastLager;
					var lagerData = Lager.ClientGlobalSettings.Lagers[lagerId];
					Lager.CurrentLager = new Lager(lagerId, lagerData.Item1, lagerData.Item2);
					if (!await Lager.CurrentLager.Load())
						await MainPage.DisplayAlert(loadingScreen.Status, "Beim laden des Lagers sind Fehler aufgetreten", "Ok");
				}
				catch (Exception e)
				{
					// Log the exception
					await Lager.Log.Exception("App", e);
					await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
				}
				// Go to the main page
				MainPage = new NavigationPage(new MainPage());
			}
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

		void DisplayStatus(Lager.InitStatus status) => loadingScreen.Status = INIT_STATUS[(int)status];

		public async Task CreateLager(string name, string password)
		{
			MainPage = new NavigationPage(loadingScreen);
			try
			{
				Lager.CurrentLager = new Lager((byte)Lager.ClientGlobalSettings.Lagers.Count, name, password);
				await Lager.CurrentLager.Init(DisplayStatus);
				loadingScreen.Status = "Lager speichern";
				await Lager.CurrentLager.Save();

				// Add lager to settings
				loadingScreen.Status = "Einstellungen speichern";
				Lager.ClientGlobalSettings.LastLager = (byte)Lager.ClientGlobalSettings.Lagers.Count;
				Lager.ClientGlobalSettings.Lagers.Add(new Tuple<string, string>(name, password));
				await Lager.ClientGlobalSettings.Save();

				// Go to the main page
				MainPage = new NavigationPage(new MainPage());
			}
			catch (Exception e)
			{
				// Log the exception
				await Lager.Log.Exception("App", e);
				await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
			}
		}
	}
}

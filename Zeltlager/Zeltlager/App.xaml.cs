using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

using Zeltlager.DataPackets;

namespace Zeltlager
{
	public partial class App : Application
	{
		LoadingScreen loadingScreen;

		public App()
		{
			InitializeComponent();

			Lager.IsClient = true;
			Lager.IoProvider = new Client.IoProvider();
			Lager.CryptoProvider = new BCCryptoProvider();
			Lager.ClientGlobalSettings = new Client.GlobalSettings();


			// Set the current lager
			//TODO Don't set default Lager
			Lager lager = new Lager(0, "Default", "pass");
			Lager.CurrentLager = lager;

			/*lager.Init();
			Tent tent = new Tent(0, "Regenbogenforellen", new List<Member>());
			DataPacket packet = new AddTentPacket(tent);
			lager.Collaborators.First().AddPacket(packet);
			var member = new Member(0, "Caro", tent, true);
			packet = new AddMemberPacket(member);
			lager.Collaborators.First().AddPacket(packet);*/
			//lager.AddTent(tent);
			//lager.AddMember(member);
			//lager.Load(Lager.IoProvider);
			//lager.Save();

			loadingScreen = new LoadingScreen();
			MainPage = loadingScreen;
		}

		protected async override void OnStart()
		{
			// Load settings
			try
			{
				loadingScreen.Status = "Einstellungen laden";
				await Lager.ClientGlobalSettings.Load();
			}
			catch (Exception e)
			{
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
				}
				catch (Exception e)
				{
					await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
				}
			}
			else
			{
				// Create lager
				try
				{
					loadingScreen.Status = "Lager erstellen";
				}
				catch (Exception e)
				{
					await MainPage.DisplayAlert(loadingScreen.Status, e.ToString(), "Ok");
				}
			}

			//FIXME Wait a bit to admire the loading screen ;)
			await System.Threading.Tasks.Task.Delay(500);
			MainPage = new NavigationPage(new MainPage());
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

using Zeltlager.DataPackets;

namespace Zeltlager
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			Lager.IsClient = true;
			Lager.IoProvider = new ClientIoProvider();
			Lager.CryptoProvider = new BCCryptoProvider();
			// Set the current lager
			//TODO Don't set default Lager
			Lager lager = new Lager(0, "Default", "pass");
			Lager.CurrentLager = lager;

			lager.Init();
			Tent tent = new Tent(0, "Regenbogenforellen", new List<Member>());
			DataPacket packet = new AddTentPacket(tent);
			lager.Collaborators.First().AddPacket(packet);
			var member = new Member(0, "Caro", tent, true);
			packet = new AddMemberPacket(member);
			lager.Collaborators.First().AddPacket(packet);
			//lager.AddTent(tent);
			//lager.AddMember(member);
			//lager.Load(Lager.IoProvider);
			//lager.Save();

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

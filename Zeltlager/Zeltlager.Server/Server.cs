using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.Server
{
	using Network;

	class Server
	{
		static void Main(string[] args)
		{
			Task.WaitAll(AsyncMain(args));
		}

		static async Task AsyncMain(string[] args)
		{
			// Load LagerManager
			LagerManager.IsClient = false;
			var io = new RootedIoProvider(new DesktopIoProvider(), Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
			LagerManager lagerManager = new LagerManager(io);
			lagerManager.NetworkClient = new TcpNetworkClient();
			lagerManager.NetworkServer = new TcpNetworkServer();
			await LagerManager.Log.Load();
			LagerManager.Log.OnMessage += Console.WriteLine;
			await lagerManager.Load();

			// Load all bundles
			foreach (var lager in lagerManager.Lagers.Values)
			{
				if (!await lager.LoadBundles())
					await LagerManager.Log.Error("Server", "Loading the bundles for lager " + lager.Id + " failed");
			}

			await LagerManager.Log.Info("Server", "Is running");

			// Let the server run
			while (true)
			{
				// Maybe add something to kill the server gracefully here?
				// Wait an hour and 10 seconds
				await Task.Delay(new TimeSpan(1, 0, 10));
			}
		}
	}
}

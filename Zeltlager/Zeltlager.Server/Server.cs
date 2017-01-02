using System;
using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.Server
{
	using Network;

	class Server
	{
		public static byte[] StringToByteArray(string hex)
		{
			byte[] bytes = new byte[hex.Length / 2];
			for (int i = 0; i < hex.Length; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}

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
			await LagerManager.Log.Info("Server", "Is running");

			// Let the server run for a while
			await Task.Delay(1000 * 60 * 60);
		}
	}
}

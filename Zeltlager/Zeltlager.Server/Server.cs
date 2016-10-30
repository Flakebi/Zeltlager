using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sockets.Plugin;

using Zeltlager.DataPackets;

namespace Zeltlager.Server
{
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
			//Task.Run(async () => await AsyncMain(args)).GetAwaiter().GetResult();
			Task.WaitAll(AsyncMain(args));
		}

		static async Task AsyncMain(string[] args)
		{
			//UdpSocketClient c = new UdpSocketClient();
			//await c.SendToAsync(new byte[] { 65 }, "192.168.1.100", 44444);

			ICryptoProvider crypto = new BCCryptoProvider();
			byte[] key = await crypto.GetRandom(CryptoConstants.SYMMETRIC_KEY_LENGTH);
			byte[] data = Encoding.UTF8.GetBytes("This is a test :)");
			byte[] mac = await crypto.ComputeMac(key, data);
			Console.WriteLine("Mac: " + mac.ToHexString());

			// Symmetric encryption
			byte[] iv = await crypto.GetRandom(CryptoConstants.SYMMETRIC_BLOCK_LENGTH);
			byte[] encrypted = await crypto.EncryptSymetric(key, iv, data);
			byte[] decrypted = await crypto.DecryptSymetric(key, iv, encrypted);
			Console.WriteLine("Result: " + Encoding.UTF8.GetString(decrypted));

			// Signature
			var keyPair = await crypto.CreateAsymmetricKey();
			var signature = await crypto.Sign(keyPair, data);
			var verification = await crypto.Verify(keyPair, signature, data);
			Console.WriteLine("Verification: " + verification);

			LagerBase.IoProvider = new RootedIoProvider(new ServerIoProvider(), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Lager"));
			await LagerBase.Log.Load();

			Task.Delay(3000).Wait();
			return;

			LagerBase lager = new LagerBase();

			/*Tent tent = new Tent(0, "Regenbogenforellen", new List<Member>());
			DataPacket packet = new AddTent(tent);
			lager.Collaborators.First().AddPacket(packet);
			packet = new AddMember(new Member(0, "Caro", tent, true));
			lager.Collaborators.First().AddPacket(packet);*/

			//await lager.Load(Lager.IoProvider);
			//await lager.Save();
		}
	}
}

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
		public static string ByteArrayToString(byte[] ba)
		{
			StringBuilder hex = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}

		public static byte[] StringToByteArray(string hex)
		{
			byte[] bytes = new byte[hex.Length / 2];
			for (int i = 0; i < hex.Length; i += 2)
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			return bytes;
		}

		static void Main(string[] args)
		{
			Task.Run(async () => await AsyncMain(args)).GetAwaiter().GetResult();
		}

		static async Task AsyncMain(string[] args)
		{
			//UdpSocketClient c = new UdpSocketClient();
			//await c.SendToAsync(new byte[] { 65 }, "192.168.1.100", 44444);

			ICryptoProvider crypto = new BCCryptoProvider();
			byte[] key = await crypto.GetRandom(CryptoConstants.SYMMETRIC_KEY_LENGTH);
			byte[] data = Encoding.UTF8.GetBytes("This is a test :)");
			byte[] mac = await crypto.ComputeMac(key, data);
			Console.WriteLine("Mac: " + ByteArrayToString(mac));

			// Symmetric encryption
			byte[] iv = await crypto.GetRandom(CryptoConstants.SYMMETRIC_BLOCK_LENGTH);
			byte[] encrypted = await crypto.EncryptSymetric(key, iv, data);
			byte[] decrypted = await crypto.DecryptSymetric(key, iv, encrypted);
			Console.WriteLine("Result: " + Encoding.UTF8.GetString(decrypted));

			// Signature
			var keyPair = await crypto.CreateAsymmetricKeys();
			var signature = await crypto.Sign(keyPair.Modulus, keyPair.PrivateKey, data);
			var verification = await crypto.Verify(keyPair.Modulus, keyPair.PublicKey, signature, data);
			Console.WriteLine("Verification: " + verification);

			Task.Delay(3000).Wait();
			return;

			Lager.IoProvider = new RootedIoProvider(new ServerIoProvider(), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Lager"));
			await Lager.Log.Load();
			Lager lager = new Lager(0, "default", "pass");

			await lager.Init(status => { });
			Tent tent = new Tent(0, "Regenbogenforellen", new List<Member>());
			DataPacket packet = new AddTent(tent);
			lager.Collaborators.First().AddPacket(packet);
			packet = new AddMember(new Member(0, "Caro", tent, true));
			lager.Collaborators.First().AddPacket(packet);

			//await lager.Load(Lager.IoProvider);
			await lager.Save();
		}
	}
}

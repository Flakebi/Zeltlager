using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			ICryptoProvider crypto = new BCCryptoProvider();
			var key = crypto.GetRandom(CryptoConstants.SYMMETRIC_KEY_LENGTH);
			var data = Encoding.UTF8.GetBytes("This is a test :)");
			var mac = crypto.ComputeMac(key, data);
			Console.WriteLine("Mac: " + ByteArrayToString(mac));

			// Symmetric encryption
			var iv = crypto.GetRandom(CryptoConstants.SYMMETRIC_BLOCK_LENGTH);
			var encrypted = crypto.EncryptSymetric(key, iv, data);
			var decrypted = crypto.DecryptSymetric(key, iv, encrypted);
			Console.WriteLine("Result: " + Encoding.UTF8.GetString(decrypted));

			// Signature
			var keyPair = crypto.CreateAsymmetricKeys();
			var signature = crypto.Sign(keyPair.Modulus, keyPair.PrivateKey, data);
			var verification = crypto.Verify(keyPair.Modulus, keyPair.PublicKey, signature, data);
			Console.WriteLine("Verification: " + verification);

			Task.Delay(3000).Wait();
			return;
			
			Lager lager = new Lager(0, "default", "pass");
			Lager.IoProvider = new RootedIoProvider(new ServerIoProvider(), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Lager"));
			Lager.CryptoProvider = new BCCryptoProvider();

			lager.Init();
			Tent tent = new Tent(0, "Regenbogenforellen", new List<Member>());
			DataPacket packet = new AddTentPacket(tent);
			lager.Collaborators.First().AddPacket(packet);
			packet = new AddMemberPacket(new Member(0, "Caro", tent, true));
			lager.Collaborators.First().AddPacket(packet);

			//lager.Load(Lager.IoProvider).GetAwaiter().GetResult();
			lager.Save().GetAwaiter().GetResult();
		}
	}
}

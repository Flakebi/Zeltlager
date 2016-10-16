using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager
{
	public class LagerBase
	{
		public static bool IsClient { get; set; }
		public static IIoProvider IoProvider { get; set; }
		public static ICryptoProvider CryptoProvider { get; set; }
		public static Log Log { get; set; }

		public byte Id { get; set; }
		public IReadOnlyList<Collaborator> Collaborators { get { return collaborators; } }

		protected List<Collaborator> collaborators = new List<Collaborator>();

		/// <summary>
		/// Packets that could not be loaded.
		/// Each tuple contains the collaborator and the packet id.
		/// </summary>
		public List<Tuple<byte, ushort>> MissingPackets { get; set; }

		// Crypto
		/// <summary>
		/// The salt used for the key derivation functions.
		/// </summary>
		protected byte[] salt;
		/// <summary>
		/// The asymmetric keys of this lager, the private key is null for the server.
		/// </summary>
		protected KeyPair asymmetricKey;

		/// <summary>
		/// The version of the data packet protocol.
		/// </summary>
		protected const byte VERSION = 0;
		protected const string GENERAL_SETTINGS_FILE = "lager.conf";

		static LagerBase()
		{
			CryptoProvider = new BCCryptoProvider();
			Log = new Log();
		}
	}
}

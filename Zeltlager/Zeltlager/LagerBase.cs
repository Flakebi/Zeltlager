using System;
using System.Collections.Generic;

namespace Zeltlager
{
	using Serialisation;

	public class LagerBase
	{
		/// <summary>
		/// The version of the data packet protocol.
		/// </summary>
		protected const byte VERSION = 0;
		protected const string GENERAL_SETTINGS_FILE = "lager.conf";

		public static bool IsClient { get; set; }
		public static IIoProvider IoProvider { get; set; }
		public static ICryptoProvider CryptoProvider { get; set; }
		public static Log Log { get; set; }

		public byte Id { get; set; }
		public Serialiser<LagerSerialisationContext> serialiser = new Serialiser<LagerSerialisationContext>();
		public IReadOnlyList<Collaborator> Collaborators { get { return collaborators; } }

		protected List<Collaborator> collaborators = new List<Collaborator>();

		//TODO Use the lager status
		/// <summary>
		/// The number of packets that were generated so far by each client.
		/// For clients, the collaborator order and packet count is the one
		/// of the server.
		/// </summary>
		public LagerStatus Status { get; set; }

		/// <summary>
		/// Packets that could not be loaded or are not yet available
		/// and should be fetched from the server.
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

		static LagerBase()
		{
			CryptoProvider = new BCCryptoProvider();
			Log = new Log();
		}

		public LagerBase()
		{
			Status = new LagerStatus();
		}
	}
}

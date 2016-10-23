using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
		public static ICryptoProvider CryptoProvider { get; set; }
		public static Log Log { get; set; }

		protected IIoProvider ioProvider;

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
		public KeyPair AsymmetricKey { get; private set; }

		static LagerBase()
		{
			CryptoProvider = new BCCryptoProvider();
			Log = new Log();
		}

		public LagerBase(IIoProvider io)
		{
			ioProvider = io;
		}

		/// <summary>
		/// Find out which bundles are currently saved on the disk.
		/// </summary>
		/// <returns>The list of operators and bundles currently saved.</returns>
		protected async Task<LagerStatus> ReadLagerStatus()
		{
			LagerStatus status = new LagerStatus();
			// Check for collaborator folders
			var folders = await ioProvider.ListContents("");
			try
			{
				for (int collaboratorId = 0;
					folders.Contains(new Tuple<string, FileType>(collaboratorId.ToString(), FileType.Folder));
					collaboratorId++)
				{
					// Read the collaborator if possible

				}
			} catch (Exception e)
			{
				await Log.Exception("LagerStatus", e);
			}
			//TODO
			return status;
		}
	}
}

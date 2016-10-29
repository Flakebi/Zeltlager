using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using DataPackets;
	using Serialisation;

	public class LagerBase : ISerialisable<LagerSerialisationContext>
	{
		/// <summary>
		/// The version of the data packet protocol.
		/// </summary>
		protected const byte VERSION = 0;
		const string LAGER_FILE = "lager.data";
		const string COLLABORATOR_FILE = "collaborator.data";

		public static bool IsClient { get; set; }
		public static ICryptoProvider CryptoProvider { get; set; }
		public static Log Log { get; set; }

		protected IIoProvider ioProvider;

		/// <summary>
		/// The data of this lager.
		/// This contains the version, the public key, salt, iv and (encrypted) the name and private key.
		/// All this data is signed with the lager private key.
		/// </summary>
		byte[] data;

		public Serialiser<LagerSerialisationContext> Serialiser { get; private set; }
		protected LagerManager manager;
		public IReadOnlyList<Collaborator> Collaborators => collaborators;

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
		//TODO Do we need this?
		public List<PacketId> MissingPackets { get; set; }

		// Crypto
		/// <summary>
		/// The asymmetric keys of this lager, the private key is null for the server.
		/// </summary>
		public KeyPair AsymmetricKey { get; protected set; }

		static LagerBase()
		{
			CryptoProvider = new BCCryptoProvider();
			Log = new Log();
		}

		public LagerBase(LagerManager manager, IIoProvider io)
		{
			this.manager = manager;
			Serialiser = new Serialiser<LagerSerialisationContext>();
			ioProvider = io;
		}

		/// <summary>
		/// Find out which bundles are currently saved on the disk and read a
		/// list of collaborators.
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
					IIoProvider rootedIo = new RootedIoProvider(ioProvider, collaboratorId.ToString());
					using (BinaryReader input = new BinaryReader(await rootedIo.ReadFile(COLLABORATOR_FILE)))
					{
						Collaborator collaborator = new Collaborator();
						LagerSerialisationContext context = new LagerSerialisationContext(manager, this);
						context.PacketId = new PacketId(collaborator);
						await Serialiser.Read(input, context, collaborator);
						// Find out how many bundles this collaborator has
						var files = await rootedIo.ListContents("");
						uint bundleCount = 0;
						while (files.Contains(new Tuple<string, FileType>(bundleCount.ToString(), FileType.File)))
							bundleCount++;
						status.BundleCount.Add(new Tuple<Collaborator, uint>(collaborator, bundleCount));
					}
				}
			} catch (Exception e)
			{
				await Log.Exception("LagerStatus", e);
			}
			return status;
		}

		async Task Deserialise()
		{
			LagerSerialisationContext context = new LagerSerialisationContext(manager, this);
			//TODO Verify and save public key
		}

		// Serialisation with a LagerSerialisationContext
		public async Task Write(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			await serialiser.Write(output, context, data);
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			// Get our lager id from the manager
			output.Write((byte)context.Manager.Lagers.IndexOf(this));
			return new Task(() => { });
		}

		public async Task Read(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			data = await serialiser.Read<byte[]>(input, context, null);
		}

		public static Task<LagerBase> ReadFromId(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			byte id = input.ReadByte();
			return Task.FromResult(context.Manager.Lagers[id]);
		}
	}
}

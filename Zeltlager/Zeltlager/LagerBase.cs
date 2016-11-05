using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Cryptography;
	using DataPackets;
	using Serialisation;

	/// <summary>
	/// Serialising a LagerBase with a LagerSerialisationContext will write
	/// the data of a lager.
	/// </summary>
	public class LagerBase : ISerialisable<LagerSerialisationContext>
	{
		/// <summary>
		/// The version of the data packet protocol.
		/// </summary>
		protected const int VERSION = 0;
		const string LAGER_FILE = "lager.data";
		protected const string COLLABORATOR_FILE = "collaborator.data";

		public LagerManager Manager { get; private set; }
		public readonly int Id;

		protected IIoProvider ioProvider;

		/// <summary>
		/// The data of this lager.
		/// This contains the version, the public key, salt, iv and (encrypted) the name and private key.
		/// All this data is signed with the lager private key.
		/// </summary>
		protected byte[] data;

		protected Serialiser<LagerSerialisationContext> serialiser;

		protected Dictionary<KeyPair, Collaborator> collaborators = new Dictionary<KeyPair, Collaborator>();
		public IReadOnlyDictionary<KeyPair, Collaborator> Collaborators => collaborators;

		/// <summary>
		/// The number of packets that were generated so far by each client.
		/// This is the state of the local data.
		/// </summary>
		protected LagerStatus Status;

		// Crypto
		/// <summary>
		/// The asymmetric keys of this lager, the private key is null for the server.
		/// </summary>
		public KeyPair AsymmetricKey { get; protected set; }

		public LagerBase(LagerManager manager, IIoProvider io, int id)
		{
			Manager = manager;
			serialiser = new Serialiser<LagerSerialisationContext>();
			ioProvider = io;
			Id = id;
		}

		/// <summary>
		/// Load the data, the lager status and the collaborators of a lager.
		/// </summary>
		public virtual async Task Load()
		{
			LagerSerialisationContext context = new LagerSerialisationContext(Manager, this);
			// Load the lager data
			using (BinaryReader input = new BinaryReader(await ioProvider.ReadFile(LAGER_FILE)))
				await serialiser.Read(input, context, this);

			Status = await ReadLagerStatus();
			collaborators = Status.BundleCount.Select(c => c.Item1).ToDictionary(c => c.Key);
		}

        public virtual async Task Save()
        {
            LagerSerialisationContext context = new LagerSerialisationContext(Manager, this);
            // Load the lager data
            using (BinaryWriter output = new BinaryWriter(await ioProvider.WriteFile(LAGER_FILE)))
                await serialiser.Write(output, context, this);
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
					Collaborator collaborator = new Collaborator();
					collaborator.Id = collaboratorId;
					LagerSerialisationContext context = new LagerSerialisationContext(Manager, this);
					context.PacketId = new PacketId(collaborator);

					using (BinaryReader input = new BinaryReader(await rootedIo.ReadFile(COLLABORATOR_FILE)))
						await serialiser.Read(input, context, collaborator);

					// Find out how many bundles this collaborator has
					var files = await rootedIo.ListContents("");
					int bundleCount = 0;
					while (files.Contains(new Tuple<string, FileType>(bundleCount.ToString(), FileType.File)))
						bundleCount++;
					status.BundleCount.Add(new Tuple<Collaborator, int>(collaborator, bundleCount));
				}
			} catch (Exception e)
			{
				await LagerManager.Log.Exception("LagerStatus", e);
			}
			return status;
		}

		async Task Verify()
		{
			// Check if the data signature is valid
			byte[] signature = new byte[CryptoConstants.SIGNATURE_LENGTH];
            byte[] signedData = new byte[data.Length - signature.Length];
			Array.Copy(data, data.Length - signature.Length, signature, 0, signature.Length);
            Array.Copy(data, signedData, signedData.Length);
            if (!await LagerManager.CryptoProvider.Verify(AsymmetricKey, signature, signedData))
				throw new InvalidDataException("The signature of the lager is wrong");
		}

		string GetBundlePath(PacketId id)
		{
			// Get the local collaborator id
			int collaboratorId = Status.BundleCount.FindIndex(c => c.Item1 == id.Creator);
			return Path.Combine(collaboratorId.ToString(), id.Bundle.Id.ToString());
		}

		public async Task<DataPacketBundle> LoadBundle(Collaborator creator, int bundleId)
		{
			DataPacketBundle bundle = new DataPacketBundle();
			bundle.Id = bundleId;
			PacketId id = new PacketId(creator, bundle);
			LagerSerialisationContext context = new LagerSerialisationContext(Manager, this);
			context.PacketId = id;

			using (BinaryReader input = new BinaryReader(await ioProvider.ReadFile(GetBundlePath(id))))
				return await serialiser.Read(input, context, bundle);
		}

		public async Task SaveBundle(PacketId id)
		{
			using (BinaryWriter output = new BinaryWriter(await ioProvider.WriteFile(GetBundlePath(id))))
				await SerialiseBundle(output, id);
		}

		protected virtual async Task SerialiseBundle(BinaryWriter output, PacketId id)
		{
			LagerSerialisationContext context = new LagerSerialisationContext(Manager, this);
			context.PacketId = id;
			await serialiser.Write(output, context, id.Bundle);
		}

		// Serialisation with a LagerSerialisationContext
		public virtual async Task Write(BinaryWriter output,
			Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			await serialiser.Write(output, context, data);
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser,
			LagerSerialisationContext context)
		{
			output.Write(Id);
			return Task.WhenAll();
		}

		public virtual async Task Read(BinaryReader input,
			Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			data = await serialiser.Read<byte[]>(input, context, null);
			using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
			{
				int version = reader.ReadInt32();
				if (version != VERSION)
					throw new InvalidDataException("The lager has an unknown version");
				AsymmetricKey = reader.ReadPublicKey();
			}
			await Verify();
		}

		public static Task<LagerBase> ReadFromId(BinaryReader input,
			Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			int id = input.ReadInt32();
			return Task.FromResult(context.Manager.Lagers[id]);
		}
	}
}

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
		public const int VERSION = 0;

		const string LAGER_FILE = "lager.data";
		protected const string COLLABORATOR_FILE = "collaborator.data";

		public LagerManager Manager { get; private set; }
		public readonly int Id;

		protected IIoProvider ioProvider;

		public LagerData Data { get; set; }

		public Serialiser<LagerSerialisationContext> Serialiser { get; private set; }

		protected Dictionary<KeyPair, Collaborator> collaborators = new Dictionary<KeyPair, Collaborator>();
		public IReadOnlyDictionary<KeyPair, Collaborator> Collaborators => collaborators;

		/// <summary>
		/// The number of packets that were generated so far by each client.
		/// This is the state of the local data.
		/// </summary>
		public LagerStatus Status { get; set; }

		/// <summary>
		/// Information about a remote lager that is used for synchronisation, if set.
		/// </summary>
		public LagerRemote Remote { get; set; }

		public LagerBase(LagerManager manager, IIoProvider io, int id)
		{
			Manager = manager;
			Serialiser = new Serialiser<LagerSerialisationContext>();
			ioProvider = io;
			Id = id;
			Status = new LagerStatus();
		}

		/// <summary>
		/// Load the data, the lager status and the collaborators of a lager.
		/// </summary>
		public virtual async Task Load()
		{
			LagerSerialisationContext context = new LagerSerialisationContext(Manager, this);
			// Load the lager data
			using (BinaryReader input = new BinaryReader(await ioProvider.ReadFile(LAGER_FILE)))
				await Serialiser.Read(input, context, this);

			await ReadLagerStatus();
		}

        public virtual async Task Save()
        {
            LagerSerialisationContext context = new LagerSerialisationContext(Manager, this);
			// Create the folder if it doesn't exist
			await ioProvider.CreateFolder("");
            // Write the lager data
            using (BinaryWriter output = new BinaryWriter(await ioProvider.WriteFile(LAGER_FILE)))
                await Serialiser.Write(output, context, this);
        }

		/// <summary>
		/// Find out which bundles are currently saved on the disk and read a
		/// list of collaborators.
		/// The collaborators list is also set by this function
		/// </summary>
		/// <returns>The list of operators and bundles currently saved.</returns>
		protected async Task ReadLagerStatus()
		{
			Status = new LagerStatus();
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
					LagerSerialisationContext context = new LagerSerialisationContext(Manager, this);
					context.PacketId = new PacketId(collaborator);

					using (BinaryReader input = new BinaryReader(await rootedIo.ReadFile(COLLABORATOR_FILE)))
						await Serialiser.Read(input, context, collaborator);

					// Find out how many bundles this collaborator has
					var files = await rootedIo.ListContents("");
					int bundleCount = 0;
					while (files.Contains(new Tuple<string, FileType>(bundleCount.ToString(), FileType.File)))
						bundleCount++;
					Status.AddBundleCount(new Tuple<KeyPair, int>(collaborator.Key, bundleCount));
					collaborators.Add(collaborator.Key, collaborator);
				}
			} catch (Exception e)
			{
				await LagerManager.Log.Exception("LagerStatus", e);
			}
		}

		Task Verify()
		{
			return Data.Verify();
		}

		string GetBundlePath(PacketId id)
		{
			// Get the local collaborator id
			int collaboratorId = Status.GetCollaboratorId(id.Creator);
			if (collaboratorId == -1)
				throw new LagerException("Can't get the bundle path for an unknown collaborator");
			return Path.Combine(collaboratorId.ToString(), id.Bundle.Id.ToString());
		}

		/// <summary>
		/// Load all bundles of this lager.
		/// </summary>
		/// <returns>
		/// The status of the lager loading.
		/// true if the lager was loaded successfully, false otherwise.
		/// </returns>
		public async Task<bool> LoadBundles()
		{
			// Read all packets
			bool success = true;
			foreach (var bundleCount in Status.BundleCount)
			{
				Collaborator collaborator = Collaborators[bundleCount.Item1];
				try
				{
					for (int i = 0; i < bundleCount.Item2; i++)
					{
						var bundle = await LoadBundle(collaborator, i);
						collaborator.AddBundle(bundle);
					}
				} catch (Exception e)
				{
					await LagerManager.Log.Exception("Bundle loading", e);
					success = false;
				}
			}
			return success;
		}

		public async Task<DataPacketBundle> LoadBundle(Collaborator creator, int bundleId)
		{
			DataPacketBundle bundle = new DataPacketBundle();
			bundle.Id = bundleId;
			PacketId id = new PacketId(creator, bundle);
			LagerSerialisationContext context = new LagerSerialisationContext(Manager, this);
			context.PacketId = id;

			using (BinaryReader input = new BinaryReader(await ioProvider.ReadFile(GetBundlePath(id))))
				return await Serialiser.Read(input, context, bundle);
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
			await Serialiser.Write(output, context, id.Bundle);
		}

		/// <summary>
		/// Add and save a bundle for a specified collaborator.
		/// </summary>
		/// <param name="collaborator">The collaborator to whom a bundle should be added.</param>
		/// <param name="bundle">The new bundle for the collaborator.</param>
		public async Task AddBundle(Collaborator collaborator, DataPacketBundle bundle)
		{
			collaborator.AddBundle(bundle);
			await SaveBundle(new PacketId(collaborator, bundle));
			Status.UpdateBundleCount(collaborator);
		}

		public async Task AddCollaborator(Collaborator collaborator)
		{
			collaborators.Add(collaborator.Key, collaborator);
			Status.AddBundleCount(new Tuple<KeyPair, int>(collaborator.Key, 0));

			// Save the lager
			await Save();

			// Save the new collaborator
			IIoProvider io = new RootedIoProvider(ioProvider, Status.GetCollaboratorId(collaborator).ToString());
			await io.CreateFolder("");
			using (BinaryWriter output = new BinaryWriter(await io.WriteFile(COLLABORATOR_FILE)))
				await Serialiser.Write(output, new LagerSerialisationContext(Manager, this), collaborator);
		}

		// Serialisation with a LagerSerialisationContext
		public virtual async Task Write(BinaryWriter output,
			Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			output.Write(Data);
			// Write server related data only if this lager is connected to a server
			output.Write(Remote != null);
			if (Remote != null)
				await serialiser.Write(output, context, Remote);
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
			Data = input.ReadLagerData();
			// Read server related data only if this lager is connected to a server
			if (input.ReadBoolean())
			{
				Remote = new LagerRemote();
				await serialiser.Read(input, context, Remote);
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

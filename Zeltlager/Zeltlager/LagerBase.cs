using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Cryptography;
	using DataPackets;
	
	/// <summary>
	/// Serialising a LagerBase with a LagerSerialisationContext will write
	/// the data of a lager.
	/// </summary>
	public class LagerBase
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
			ioProvider = io;
			Id = id;
			Status = new LagerStatus();
		}

		/// <summary>
		/// Load the data, the lager status and the collaborators of a lager.
		/// </summary>
		public virtual async Task Load()
		{
			// Load the lager data
			using (BinaryReader input = new BinaryReader(await ioProvider.ReadFile(LAGER_FILE)))
				await Serialiser.Read(input, context, this);

			await ReadLagerStatus();
		}

		public virtual async Task Save()
		{
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

		/// <summary>
		/// Unload all bundles and reset the lager.
		/// The lager status will still be loaded (like after Load() was called)
		/// but the bundles will not be in memory.
		/// </summary>
		public virtual void Unload()
		{
			foreach (var c in collaborators.Values)
				c.Unload();
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
			for (int b = 0; b < Status.BundleCount.Count; b++)
			{
				var bundleCount = Status.BundleCount[b];
				Collaborator collaborator = Collaborators[bundleCount.Item1];
				// Be sure that no bundles are loaded for the collaborator
				collaborator.Unload();
				int i = 0;
				try
				{
					for (; i < bundleCount.Item2; i++)
					{
						var bundle = await LoadBundle(collaborator, i);
						await bundle.Verify(collaborator);
						collaborator.AddBundle(bundle);
					}
				} catch (Exception e)
				{
					await LagerManager.Log.Exception("Loading bundle " + i + " for collaborator " + collaborator + " in lager " + Id, e);
					success = false;
				}
				Status.UpdateBundleCount(collaborator);
			}
			return success;
		}

		public async Task<DataPacketBundle> LoadBundle(Collaborator creator, int bundleId)
		{
			DataPacketBundle bundle = new DataPacketBundle();
			bundle.Id = bundleId;
			PacketId id = new PacketId(creator, bundle);
			LagerSerialisationContext context = new LagerSerialisationContext(this);
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
			LagerSerialisationContext context = new LagerSerialisationContext(this);
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
				await Serialiser.Write(output, new LagerSerialisationContext(this), collaborator);
		}

		public async Task RemovePacket(PacketId id)
		{
			
		}
	}
}

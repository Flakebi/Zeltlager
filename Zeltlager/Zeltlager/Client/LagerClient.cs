using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
	using Cryptography;
	using DataPackets;
	using Serialisation;

	/// <summary>
	/// Serialising a LagerClient with a LagerClientSerialisationContext will write
	/// the password of a lager,
	/// the collaborator private key,
	/// the id of this lager on the server,
	/// the order of the collaborators on the server (as a list),
	/// the lager status of the server.
	/// Reading with a LagerClientSerialisationContext will also create the collaborator list.
	/// </summary>
	public class LagerClient : LagerBase, ISerialisable<LagerSerialisationContext>, ISerialisable<LagerClientSerialisationContext>
	{
		public enum InitStatus
		{
			CreateSymmetricKey,
			CreateGameAsymmetricKey,
			CreateCollaboratorAsymmetricKey,
			Ready
		}

		const string CLIENT_LAGER_FILE = "client.data";

		public string Name { get; set; }
		public IReadOnlyList<Member> Members => members;
		public IReadOnlyList<Tent> Tents => tents;

		// Subspaces
		public Competition.CompetitionHandler CompetitionHandler { get; private set; }
		public Erwischt.Erwischt Erwischt { get; private set; }
		public Calendar.Calendar Calendar { get; private set; }

		public Serialiser<LagerClientSerialisationContext> ClientSerialiser { get; private set; }

		List<Member> members = new List<Member>();
		List<Tent> tents = new List<Tent>();

		/// <summary>
		/// The collaborator that we are.
		/// </summary>
		public Collaborator OwnCollaborator { get; private set; }
		/// <summary>
		/// The private key of our own collaborator.
		/// </summary>
		KeyPair ownCollaboratorPrivateKey;

		/// <summary>
		/// The id of this lager on the server.
		/// </summary>
		int? serverId;

		/// <summary>
		/// The number of packets that were generated so far by each client.
		/// The collaborator order and packet count is the one
		/// of the server.
		/// </summary>
		LagerStatus serverStatus;

		// Crypto
		/// <summary>
		/// The salt used for the key derivation functions.
		/// </summary>
		byte[] salt;
		public byte[] SymmetricKey { get; private set; }

		/// <summary>
		/// The password supplied by the user and used to generate the shared keys.
		/// </summary>
		string password;

		public LagerClient(LagerManager manager, IIoProvider ioProvider, int id) :
			base(manager, ioProvider, id)
		{
			ClientSerialiser = new Serialiser<LagerClientSerialisationContext>();

			CompetitionHandler = new Competition.CompetitionHandler(this);
			Erwischt = new Erwischt.Erwischt(this);
			Calendar = new Calendar.Calendar();
		}

		/// <summary>
		/// Creates a new lager.
		/// </summary>
		public async Task Init(string name, string password, Action<InitStatus> statusUpdate)
		{
			Name = name;
			this.password = password;

			// Create the keys for this instance
			statusUpdate?.Invoke(InitStatus.CreateSymmetricKey);
			salt = await LagerManager.CryptoProvider.GetRandom(CryptoConstants.SALT_LENGTH);
			SymmetricKey = await LagerManager.CryptoProvider.DeriveSymmetricKey(password, salt);
			statusUpdate?.Invoke(InitStatus.CreateGameAsymmetricKey);
			AsymmetricKey = await LagerManager.CryptoProvider.CreateAsymmetricKey();

			// Create the keys for our own collaborator
			statusUpdate?.Invoke(InitStatus.CreateCollaboratorAsymmetricKey);
			ownCollaboratorPrivateKey = await LagerManager.CryptoProvider.CreateAsymmetricKey();
			OwnCollaborator = new Collaborator(ownCollaboratorPrivateKey);
			collaborators.Add(ownCollaboratorPrivateKey, OwnCollaborator);
			// Save the new collaborator
			IIoProvider io = new RootedIoProvider(ioProvider, OwnCollaborator.Id.ToString());
			using (BinaryWriter output = new BinaryWriter(await io.WriteFile(COLLABORATOR_FILE)))
				await serialiser.Write(output, new LagerSerialisationContext(Manager, this), OwnCollaborator);

			// Set the lager status
			Status = new LagerStatus();
			Status.BundleCount.Add(new Tuple<Collaborator, int>(OwnCollaborator, 0));

			// Save the lager
			await Save();

			// Add the collaborator to his own list
			var context = new LagerClientSerialisationContext(Manager, this);
			context.PacketId = new PacketId(OwnCollaborator);
			var packet = await AddCollaborator.Create(ClientSerialiser, context, OwnCollaborator);
			await AddPacket(packet);

			statusUpdate?.Invoke(InitStatus.Ready);
		}

		public override async Task Load()
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			// Load the lager client data
			using (BinaryReader input = new BinaryReader(await ioProvider.ReadFile(CLIENT_LAGER_FILE)))
				await ClientSerialiser.Read(input, context, this);

			await base.Load();
			var oldCollaborators = collaborators;

			// Unite the read collaborators of the lager status and the server lager status
			// Only key and id are available in the collaborators in the server status and collaborator
			// list so take the them and write it into the collaborators of the lager status.
			collaborators = oldCollaborators.Select(c =>
			{
				var cNew = collaborators[c.Key];
				c.Value.Id = cNew.Id;
				c.Value.Key = cNew.Key;
				return c.Value;
			}).ToDictionary(c => c.Key);

			if (serverId.HasValue)
			{
				var newBundleCount = serverStatus.BundleCount
					.Select(c => new Tuple<Collaborator, int>(collaborators[c.Item1.Key], c.Item2));
				serverStatus.BundleCount.Clear();
				serverStatus.BundleCount.AddRange(newBundleCount);
			}
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
			foreach (var collaborator in Status.BundleCount)
			{
				try
				{
					for (int i = 0; i < collaborator.Item2; i++)
					{
						var bundle = await LoadBundle(collaborator.Item1, i);
						collaborator.Item1.AddBundle(i, bundle);
					}
				}
				catch (Exception e)
				{
					await LagerManager.Log.Exception("Bundle loading", e);
					success = false;
				}
			}
			return success;
		}

		public override async Task Save()
		{
			await base.Save();

			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			// Load the lager client data
			using (BinaryWriter output = new BinaryWriter(await ioProvider.WriteFile(CLIENT_LAGER_FILE)))
				await ClientSerialiser.Write(output, context, this);
		}

		/// <summary>
		/// Assemble the packet history from all collaborators.
		/// This orders the packets in chronological order and removes packet bundles.
		/// </summary>
		/// <returns>The flat history of packets.</returns>
		async Task<List<DataPacket>> GetHistory()
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			return (await Task.WhenAll(collaborators.Values.Select(async col =>
			{
				context.PacketId = new PacketId(col);
				return (await Task.WhenAll(col.Bundles.Values.Select(b =>
				{
					context.PacketId = context.PacketId.Clone(b);
					return b.GetPackets(context);
				}))).SelectMany(p => p).Select(p => p.Item2);
			})))
				// Use OrderBy which is a stable sorting algorithm
				.SelectMany(p => p)
				.OrderBy(packet => packet.Timestamp).ToList();
		}

		/// <summary>
		/// Apply the whole history.
		/// </summary>
		/// <returns>
		/// Returns true if the whole history was applied successfully,
		/// false if an error occured.
		/// </returns>
		public async Task<bool> ApplyHistory()
		{
			List<DataPacket> history = await GetHistory();
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			bool success = true;
			foreach (var packet in history)
			{
				try
				{
					context.PacketId = packet.Id;
					await packet.Deserialise(ClientSerialiser, context);
				}
				catch (Exception e)
				{
					// Log the exception
					await LagerManager.Log.Exception("Lager", e);
					success = false;
				}
			}
			return success;
		}

		/// <summary>
		/// Adds a packet to our own collaborator and applies the packet.
		/// </summary>
		public async Task AddPacket(DataPacket packet)
		{
			DataPacketBundle bundle;
			// Check if a usable bundle exists
			// A bundle is usable if it was not already synchronised with the server
			// and if it contains free space.
			int maxBundleId = OwnCollaborator.Bundles.Any() ? OwnCollaborator.Bundles.Keys.Max() : -1;
			if (OwnCollaborator.Bundles.Any() &&
				(serverStatus == null || serverStatus.BundleCount.First(c => c.Item1 == OwnCollaborator).Item2 < maxBundleId) &&
				OwnCollaborator.Bundles[maxBundleId].Size < DataPacketBundle.MAX_PACKET_SIZE)
				bundle = OwnCollaborator.Bundles[maxBundleId];
			else
			{
				bundle = new DataPacketBundle();
				bundle.Id = maxBundleId + 1;
				OwnCollaborator.AddBundle(bundle.Id, bundle);

			}
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			PacketId id = new PacketId(OwnCollaborator, bundle, bundle.Packets == null ? 0 : bundle.Packets.Count);
			context.PacketId = id;
			await bundle.AddPacket(context, packet);

			// First, write the packet to disk
			await SaveBundle(id);

			// Then deserialise it to apply it
			await packet.Deserialise(ClientSerialiser, context);
		}

		public void AddMember(Member member)
		{
			if (Members.Any(m => m.Id == member.Id))
				throw new InvalidOperationException("A member with this id exists already.");
			members.Add(member);
		}

		public void RemoveMember(Member member)
		{
			if (!members.Remove(member))
				throw new InvalidOperationException("A member with this id wasn't found for deletion.");
		}

		public void AddTent(Tent tent)
		{
			if (Tents.Any(t => t.Id == tent.Id))
				throw new InvalidOperationException("A tent with this id exists already.");
			tents.Add(tent);
		}

		public void RemoveTent(Tent tent)
		{
			if (!tents.Remove(tent))
				throw new InvalidOperationException("A tent with this id wasn't found for deletion.");
		}

		public Tent GetTentFromDisplay(string display)
		{
			Tent t = null;
			// find correct tent from display string
			foreach (Tent tent in Tents)
			{
				if (tent.Display == display)
				{
					t = tent;
					break;
				}
			}
			return t;
		}

		/// <summary>
		/// Fill the data array.
		/// </summary>
		async Task Serialise()
		{
			if (data == null)
			{
				// Serialise the encrypted data
				MemoryStream mem = new MemoryStream();
				using (BinaryWriter writer = new BinaryWriter(mem))
				{
					writer.Write(Name);
					writer.WritePrivateKey(AsymmetricKey);
				}
				byte[] iv = await LagerManager.CryptoProvider.GetRandom(CryptoConstants.IV_LENGTH);
				byte[] encryptedData = await LagerManager.CryptoProvider.EncryptSymetric(SymmetricKey, iv, mem.ToArray());

				// Serialise the unencrypted data
				mem = new MemoryStream();
				using (BinaryWriter writer = new BinaryWriter(mem))
				{
					writer.Write(VERSION);
					writer.WritePublicKey(AsymmetricKey);
					writer.Write(salt);
					writer.Write(iv);
					writer.Write(encryptedData);
					writer.Flush();

					// Sign the data
					byte[] signature = await LagerManager.CryptoProvider.Sign(AsymmetricKey, mem.ToArray());
					writer.Write(signature);
				}
				data = mem.ToArray();
			}
		}

		/// <summary>
		/// Decrypt the data array.
		/// </summary>
		async Task Deserialise()
		{
			// Read the encrypted data
			byte[] iv;
			byte[] encryptedData;
			using (BinaryReader input = new BinaryReader(new MemoryStream(data)))
			{
				// Version
				input.ReadInt32();
				input.ReadPublicKey();
				salt = input.ReadBytes(CryptoConstants.SALT_LENGTH);
				iv = input.ReadBytes(CryptoConstants.IV_LENGTH);
				encryptedData = input.ReadBytes((int)(input.BaseStream.Length
					- input.BaseStream.Position - CryptoConstants.SIGNATURE_LENGTH));
			}

			// Decrypt the data
			SymmetricKey = await LagerManager.CryptoProvider.DeriveSymmetricKey(password, salt);
			byte[] unencryptedData = await LagerManager.CryptoProvider.DecryptSymetric(SymmetricKey, iv, encryptedData);
			using (BinaryReader input = new BinaryReader(new MemoryStream(unencryptedData)))
			{
				Name = input.ReadString();
				AsymmetricKey = input.ReadPrivateKey();
			}
		}

		// Serialisation with a LagerSerialisationContext
		public override async Task Write(BinaryWriter output,
			Serialiser<LagerSerialisationContext> serialiser,
			LagerSerialisationContext context)
		{
			await Serialise();
			await base.Write(output, serialiser, context);
		}

		public override async Task Read(BinaryReader input,
			Serialiser<LagerSerialisationContext> serialiser,
			LagerSerialisationContext context)
		{
			await base.Read(input, serialiser, context);
			await Deserialise();
		}

		// Serialisation with a LagerClientSerialisationContext
		public async Task Write(BinaryWriter output,
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context)
		{
			output.Write(password);
			output.WritePrivateKey(ownCollaboratorPrivateKey);
			// Write server related data only if this lager is connected to a server
			await serialiser.Write(output, context, serverId);
			if (serverId.HasValue)
			{
				output.Write(serverStatus.BundleCount.Count);
				foreach (var c in serverStatus.BundleCount)
					output.WritePublicKey(c.Item1.Key);

				// Serialise the server status with a normal serialisetion context.
				LagerSerialisationContext lagerContext = new LagerSerialisationContext(Manager, this);
				await this.serialiser.Write(output, lagerContext, serverStatus);
			}
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			throw new InvalidOperationException("Use a LagerSerialisationContext to write the id");
		}

		public async Task Read(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			collaborators = new Dictionary<KeyPair, Collaborator>();

			password = input.ReadString();
			ownCollaboratorPrivateKey = input.ReadPrivateKey();
			serverId = await serialiser.Read(input, context, serverId);
			// Read server related data only if this lager is connected to a server
			if (serverId.HasValue)
			{
				int count = input.ReadInt32();
				serverStatus = new LagerStatus();
				for (int i = 0; i < count; i++)
				{
					KeyPair key = input.ReadPublicKey();
					// Check if it is our own collaborator
					if (key == ownCollaboratorPrivateKey)
						key = ownCollaboratorPrivateKey;
					Collaborator collaborator = new Collaborator(key);
					collaborator.Id = i;
					serverStatus.BundleCount.Add(new Tuple<Collaborator, int>(collaborator, 0));
					if (key == ownCollaboratorPrivateKey)
						OwnCollaborator = collaborator;
				}

				// Add collaborators to collaborator list
				foreach (var c in serverStatus.BundleCount)
					collaborators.Add(c.Item1.Key, c.Item1);

				// Deserialise the server status with a normal serialisetion context.
				LagerSerialisationContext lagerContext = new LagerSerialisationContext(Manager, this);
				await this.serialiser.Read(input, lagerContext, serverStatus);
			}
			else
			{
				OwnCollaborator = new Collaborator(ownCollaboratorPrivateKey);
				collaborators.Add(ownCollaboratorPrivateKey, OwnCollaborator);
			}
		}

		public static Task<LagerClient> ReadFromId(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			throw new InvalidOperationException("Use a LagerSerialisationContext to write from an id");
		}
	}
}

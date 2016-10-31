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
	public class LagerClient : LagerBase, ISerialisable<LagerSerialisationContext>
	{
		public enum InitStatus
		{
			CreateSymmetricKey,
			CreateGameAsymmetricKey,
			CreateCollaboratorAsymmetricKey,
			Ready
		}

		const string CLIENT_LAGER_FILE = "client.data";

		public static ClientSettings Settings { get; private set; }

		public string Name { get; set; }
		public IReadOnlyList<Member> Members => members;
		public IReadOnlyList<Tent> Tents => tents;

		// Subspaces
		public Competition.CompetitionHandler CompetitionHandler { get; private set; }
		public Erwischt.Erwischt Erwischt { get; private set; }
		public Calendar.Calendar Calendar { get; private set; }

		Serialiser<LagerClientSerialisationContext> clientSerialiser;

		List<Member> members = new List<Member>();
		List<Tent> tents = new List<Tent>();

		/// <summary>
		/// The collaborator that we are.
		/// </summary>
		Collaborator ownCollaborator;
		/// <summary>
		/// The private key of our own collaborator.
		/// </summary>
		KeyPair ownCollaboratorPrivateKey;
		/// <summary>
		/// Bundles that were created but not yet sent to the server.
		/// </summary>
		List<DataPacketBundle> newBundles = new List<DataPacketBundle>();

		/// <summary>
		/// The id of this lager on the server.
		/// </summary>
		int? serverId;

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
			clientSerialiser = new Serialiser<LagerClientSerialisationContext>();

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
			statusUpdate(InitStatus.CreateSymmetricKey);
			salt = await LagerManager.CryptoProvider.GetRandom(CryptoConstants.SALT_LENGTH);
			SymmetricKey = await LagerManager.CryptoProvider.DeriveSymmetricKey(password, salt);
			statusUpdate(InitStatus.CreateGameAsymmetricKey);
			AsymmetricKey = await LagerManager.CryptoProvider.CreateAsymmetricKey();

			// Create the keys for our own collaborator
			statusUpdate(InitStatus.CreateCollaboratorAsymmetricKey);
			ownCollaboratorPrivateKey = await LagerManager.CryptoProvider.CreateAsymmetricKey();
			ownCollaborator = new Collaborator(ownCollaboratorPrivateKey);
			collaborators[ownCollaboratorPrivateKey] = ownCollaborator;

            // Save the lager
            await Save();

			// Add the collaborator to his own list
			var context = new LagerClientSerialisationContext(Manager, this);
			context.PacketId = new PacketId(ownCollaborator);
			var packet = await AddCollaborator.Create(clientSerialiser, context, ownCollaborator);
			await AddPacket(packet);

			statusUpdate(InitStatus.Ready);
		}

		public override async Task Load()
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			// Load the lager client data
			using (BinaryReader input = new BinaryReader(await ioProvider.ReadFile(CLIENT_LAGER_FILE)))
				await clientSerialiser.Read(input, context, this);

            // Save the status
            LagerStatus status = Status;
            await base.Load();
            Status = status;

			//TODO Anything more?
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
            //TODO Read all packets
            return false;
        }

        public override async Task Save()
        {
            
        }

		/// <summary>
		/// Assemble the packet history from all collaborators.
		/// This orders the packets in chronological order and removes packet bundles.
		/// </summary>
		/// <returns>The flat history of packets.</returns>
		List<DataPacket> GetHistory()
		{
			return collaborators.Values.Select(col =>
					col.Bundles.Values.Select(b => b.Packets).SelectMany(p => p)
				)
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
			List<DataPacket> history = GetHistory();
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			bool success = true;
			foreach (var packet in history)
			{
				try
				{
					context.PacketId = packet.Id;
					await packet.Deserialise(clientSerialiser, context);
				} catch (Exception e)
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
			if (!newBundles.Any() || newBundles.Last().Packets.Count == DataPacketBundle.MAX_PACKETS)
			{
				bundle = new DataPacketBundle();
				newBundles.Add(bundle);
			} else
				bundle = newBundles.Last();
			bundle.AddPacket(packet);

			// First, write the packet to disk
			//TODO Save newPackets
			//await ownCollaborator.SavePacket(rootedIo, SymmetricKey, ownCollaborator.Bundles.Count - 1);

			// Then deserialise it to apply it
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			context.PacketId = new PacketId(ownCollaborator, bundle, bundle.Packets.Count - 1);
			await packet.Deserialise(clientSerialiser, context);
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

		// Decrypt the data array.
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
				encryptedData = input.ReadBytes((int)(input.BaseStream.Length - input.BaseStream.Position));
			}

			// Decrypt the data
			SymmetricKey = await LagerManager.CryptoProvider.DeriveSymmetricKey(password, salt);
			byte[] unencryptedData = await LagerManager.CryptoProvider.DecryptSymetric(SymmetricKey, iv, encryptedData);
			using (BinaryReader input = new BinaryReader(new MemoryStream(data)))
			{
				Name = input.ReadString();
				AsymmetricKey = input.ReadPrivateKey();
			}
		}

        // Serialisation with a LagerSerialisationContext
		public override async Task Write(BinaryWriter output, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			await Serialise();
			await base.Write(output, serialiser, context);
		}

		public override async Task Read(BinaryReader input, Serialiser<LagerSerialisationContext> serialiser, LagerSerialisationContext context)
		{
			await base.Read(input, serialiser, context);
			await Deserialise();
		}

		// Serialisation with a LagerClientSerialisationContext
		public async Task Write(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			output.Write(password);
			output.WritePrivateKey(ownCollaboratorPrivateKey);
			// Write server related data only if this lager is connected to a server
			await serialiser.Write(output, context, serverId);
			if (serverId.HasValue)
			{
				output.Write(Status.BundleCount.Count);
				foreach (var c in Status.BundleCount)
					output.WritePublicKey(c.Item1.Key);

				await serialiser.Write(output, context, Status);
			}
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			throw new InvalidOperationException("Use a LagerSerialisationContext to write the id");
		}

		public async Task Read(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			collaborators.Clear();

			password = input.ReadString();
			ownCollaboratorPrivateKey = input.ReadPrivateKey();
			serverId = await serialiser.Read(input, context, serverId);
			// Read server related data only if this lager is connected to a server
			if (serverId.HasValue)
			{
				int count = input.ReadInt32();
				Status = new LagerStatus();
				for (int i = 0; i < count; i++)
				{
					KeyPair key = input.ReadPublicKey();
					// Check if it is our own collaborator
					if (key == ownCollaboratorPrivateKey)
						key = ownCollaboratorPrivateKey;
					Collaborator collaborator = new Collaborator(key);
					Status.BundleCount.Add(new Tuple<Collaborator, int>(collaborator, 0));
					if (key == ownCollaboratorPrivateKey)
						ownCollaborator = collaborator;
				}

				// Add collaborators to collaborator list
				foreach (var c in Status.BundleCount)
					collaborators.Add(c.Item1.Key, c.Item1);

				await serialiser.Read(input, context, Status);
			} else
			{
				ownCollaborator = new Collaborator(ownCollaboratorPrivateKey);
				collaborators.Add(ownCollaboratorPrivateKey, ownCollaborator);
			}
		}

		public static Task<LagerClient> ReadFromId(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			throw new InvalidOperationException("Use a LagerSerialisationContext to write from an id");
		}
	}
}

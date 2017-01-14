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

		public IReadOnlyList<Member> Members => members;
		public IReadOnlyList<Tent> Tents => tents;
		public IReadOnlyList<Member> Supervisors { get { return new List<Member>(Members.Where(x => x.Supervisor == true)); } }

		// Subspaces
		public Competition.CompetitionHandler CompetitionHandler { get; private set; }
		public Erwischt.Erwischt Erwischt { get; private set; }
		public Calendar.Calendar Calendar { get; private set; }

		public Serialiser<LagerClientSerialisationContext> ClientSerialiser { get; private set; }
			= new Serialiser<LagerClientSerialisationContext>();

		List<Member> members = new List<Member>();
		List<Tent> tents = new List<Tent>();

		/// <summary>
		/// The collaborator that we are.
		/// </summary>
		public Collaborator OwnCollaborator { get; private set; }

		/// <summary>
		/// The password supplied by the user and used to generate the shared keys.
		/// </summary>
		string password;

		public LagerClient(LagerManager manager, IIoProvider ioProvider, int id) :
			base(manager, ioProvider, id)
		{
			CompetitionHandler = new Competition.CompetitionHandler(this);
			Erwischt = new Erwischt.Erwischt(this);
			Calendar = new Calendar.Calendar();
		}

		/// <summary>
		/// Creates a new lager.
		/// </summary>
		public async Task InitLocal(string name, string password, Action<InitStatus> statusUpdate)
		{
			data = new LagerData();
			data.Name = name;
			this.password = password;

			// Create the keys for this instance
			statusUpdate?.Invoke(InitStatus.CreateSymmetricKey);
			data.Salt = await LagerManager.CryptoProvider.GetRandom(CryptoConstants.SALT_LENGTH);
			data.SymmetricKey = await LagerManager.CryptoProvider.DeriveSymmetricKey(password, data.Salt);
			statusUpdate?.Invoke(InitStatus.CreateGameAsymmetricKey);
			data.AsymmetricKey = await LagerManager.CryptoProvider.CreateAsymmetricKey();

			await Init(statusUpdate);
		}

		public async Task InitFromServer(int serverId, LagerData data, string password, Action<InitStatus> statusUpdate)
		{
			Remote = new LagerRemote(serverId);
			this.data = data;
			this.password = password;

			await Init(statusUpdate);
		}

		async Task Init(Action<InitStatus> statusUpdate)
		{
			// Create the keys for our own collaborator
			statusUpdate?.Invoke(InitStatus.CreateCollaboratorAsymmetricKey);
			KeyPair ownCollaboratorPrivateKey = await LagerManager.CryptoProvider.CreateAsymmetricKey();
			OwnCollaborator = new Collaborator(ownCollaboratorPrivateKey);
			collaborators.Add(OwnCollaborator.Key, OwnCollaborator);

			// Set the lager status
			Status = new LagerStatus();
			Status.BundleCount.Add(new Tuple<KeyPair, int>(OwnCollaborator.Key, 0));

			// Save the lager
			await Save();

			// Save the new collaborator
			IIoProvider io = new RootedIoProvider(ioProvider, Status.BundleCount.FindIndex(c => c.Item1 == OwnCollaborator.Key).ToString());
			await io.CreateFolder("");
			using (BinaryWriter output = new BinaryWriter(await io.WriteFile(COLLABORATOR_FILE)))
				await serialiser.Write(output, new LagerSerialisationContext(Manager, this), OwnCollaborator);

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

			// Correct our own collaborator with the read key (which is stored
			// temporarily in OwnCollaborator).
			KeyPair ownCollaboratorPrivateKey = OwnCollaborator.Key;
			OwnCollaborator = collaborators[ownCollaboratorPrivateKey];
			OwnCollaborator.Key = ownCollaboratorPrivateKey;
			collaborators.Remove(OwnCollaborator.Key);
			collaborators.Add(OwnCollaborator.Key, OwnCollaborator);
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
						collaborator.AddBundle(i, bundle);
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
				return (await Task.WhenAll(col.Bundles.Values.Select(
					b => b.GetPackets(context)
				))).SelectMany(p => p);
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
			    (Remote == null || Remote.Status.BundleCount.First(c => c.Item1 == OwnCollaborator.Key).Item2 < maxBundleId) &&
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

		public Member GetMemberFromString(string memberstring)
		{
			return Members.First(x => x.ToString() == memberstring);
		}

		/// <summary>
		/// Create some test data if a new lager is created
		/// </summary>
		public async Task CreateTestData()
		{
			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			context.PacketId = new PacketId(OwnCollaborator);

			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
			    new Tent(null, 0, "Tiger", false, new List<Member>(), context.LagerClient)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Tent(null, 1, "Giraffen", false, new List<Member>(), context.LagerClient)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Tent(null, 2, "Seepferdchen", false, new List<Member>(), context.LagerClient)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Tent(null, 3, "Pinguine", false, new List<Member>(), context.LagerClient)));

			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Anna", Tents.Skip(new Random().Next(0, Tents.Count)).First(), true, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Bernd", Tents.Skip(new Random().Next(0, Tents.Count)).First(), true, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Claudius", Tents.Skip(new Random().Next(0, Tents.Count)).First(), false, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Don", Tents.Skip(new Random().Next(0, Tents.Count)).First(), false, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Emily", Tents.Skip(new Random().Next(0, Tents.Count)).First(), false, this)));
			await AddPacket(await DataPackets.AddPacket.Create(ClientSerialiser, context,
				new Member(null, "Franz", Tents.Skip(new Random().Next(0, Tents.Count)).First(), false, this)));
		}

		// Serialisation with a LagerSerialisationContext
		public override async Task Write(BinaryWriter output,
			Serialiser<LagerSerialisationContext> serialiser,
			LagerSerialisationContext context)
		{
			await data.Serialise();
			await base.Write(output, serialiser, context);
		}

		public override async Task Read(BinaryReader input,
			Serialiser<LagerSerialisationContext> serialiser,
			LagerSerialisationContext context)
		{
			await base.Read(input, serialiser, context);
			await data.Decrypt(password);
		}

		// Serialisation with a LagerClientSerialisationContext
		public Task Write(BinaryWriter output,
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context)
		{
			output.Write(password);
			output.WritePrivateKey(OwnCollaborator.Key);
			return Task.WhenAll();
		}

		public Task WriteId(BinaryWriter output, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			throw new InvalidOperationException("Use a LagerSerialisationContext to write the id");
		}

		public Task Read(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			password = input.ReadString();
			KeyPair ownCollaboratorPrivateKey = input.ReadPrivateKey();
			OwnCollaborator = new Collaborator(ownCollaboratorPrivateKey);
			return Task.WhenAll();
		}

		public static Task<LagerClient> ReadFromId(BinaryReader input, Serialiser<LagerClientSerialisationContext> serialiser, LagerClientSerialisationContext context)
		{
			throw new InvalidOperationException("Use a LagerSerialisationContext to write from an id");
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
	using Cryptography;
	using DataPackets;
	using Network;
	using Requests = CommunicationPackets.Requests;
	using Responses = CommunicationPackets.Responses;
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
	public class LagerClient : LagerBase, ISerialisable<LagerSerialisationContext>, ISerialisable<LagerClientSerialisationContext>, ISearchable
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

		public LagerClientManager ClientManager { get; private set; }

		List<Member> members;
		List<Tent> tents;

		/// <summary>
		/// The collaborator that we are.
		/// </summary>
		public Collaborator OwnCollaborator { get; private set; }

		// Searchable implementation
		public string SearchableText => Data.Name;
		public string SearchableDetail => password;

		/// <summary>
		/// The password supplied by the user and used to generate the shared keys.
		/// </summary>
		string password;

		public LagerClient(LagerClientManager manager, IIoProvider ioProvider, int id) :
			base(manager, ioProvider, id)
		{
			ClientManager = manager;
			Reset();
		}

		/// <summary>
		/// Resets all loaded data of this lager instance.
		/// This should be used before reloading the history.
		/// </summary>
		public void Reset()
		{
			members = new List<Member>();
			tents = new List<Tent>();
			
			CompetitionHandler = new Competition.CompetitionHandler(this);
			Erwischt = new Erwischt.Erwischt(this);
			Calendar = new Calendar.Calendar(this);
		}

		/// <summary>
		/// Creates a new lager.
		/// </summary>
		public async Task InitLocal(string name, string password, Action<InitStatus> statusUpdate)
		{
			Data = new LagerData();
			Data.Name = name;
			this.password = password;

			// Create the keys for this instance
			statusUpdate?.Invoke(InitStatus.CreateSymmetricKey);
			Data.Salt = await LagerManager.CryptoProvider.GetRandom(CryptoConstants.SALT_LENGTH);
			Data.SymmetricKey = await LagerManager.CryptoProvider.DeriveSymmetricKey(password, Data.Salt);
			statusUpdate?.Invoke(InitStatus.CreateGameAsymmetricKey);
			Data.AsymmetricKey = await LagerManager.CryptoProvider.CreateAsymmetricKey();

			await Init(statusUpdate);
		}

		public async Task InitFromServer(int serverId, LagerData data, string password, Action<InitStatus> statusUpdate)
		{
			Remote = new LagerRemote(serverId);
			Data = data;
			this.password = password;

			await Init(statusUpdate);
		}

		async Task Init(Action<InitStatus> statusUpdate)
		{
			Status = new LagerStatus();

			// Create the keys for our own collaborator
			statusUpdate?.Invoke(InitStatus.CreateCollaboratorAsymmetricKey);
			KeyPair ownCollaboratorPrivateKey = await LagerManager.CryptoProvider.CreateAsymmetricKey();
			OwnCollaborator = new Collaborator(ownCollaboratorPrivateKey);
			await AddCollaborator(OwnCollaborator);

			// Add the collaborator to his own list
			var context = new LagerClientSerialisationContext(Manager, this);
			context.PacketId = new PacketId(OwnCollaborator);
			var packet = await DataPackets.AddCollaborator.Create(ClientSerialiser, context, OwnCollaborator);
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

		public override async Task Save()
		{
			await base.Save();

			LagerClientSerialisationContext context = new LagerClientSerialisationContext(Manager, this);
			// Load the lager client data
			using (BinaryWriter output = new BinaryWriter(await ioProvider.WriteFile(CLIENT_LAGER_FILE)))
				await ClientSerialiser.Write(output, context, this);
		}

		/// <summary>
		/// Synchronise this lager with the remote lager.
		/// 1. Update the remote lager status from the server
		/// 2. Fetch the data of all new collaborators
		/// 3. Download all new bundles
		/// 4. Upload our own new bundles
		/// </summary>
		/// <param name="statusUpdate">Status update.</param>
		public async Task Synchronise(Action<NetworkStatus> statusUpdate)
		{
			// Open a connection
			statusUpdate?.Invoke(NetworkStatus.Connecting);
			INetworkConnection connection = null;
			try
			{
				connection = await Manager.NetworkClient.OpenConnection(
					ClientManager.Settings.ServerAddress, LagerManager.PORT);

				// Request the lager status
				statusUpdate?.Invoke(NetworkStatus.LagerStatusRequest);
				await connection.WritePacket(await Requests.LagerStatus.Create(this));
				var lagerStatusResponse = await connection.ReadPacket() as Responses.LagerStatus;
				if (lagerStatusResponse == null)
					throw new LagerException("Got no lager status as response");
				await lagerStatusResponse.ReadRemoteStatus(this);
				await Save();

				// Check for new collaborators
				statusUpdate?.Invoke(NetworkStatus.CollaboratorDataRequest);
				// First, send all requests then wait for the answers
				var missingCollaborators = Remote.Status.BundleCount.Select(c => c.Item1).Except(Status.BundleCount.Select(c => c.Item1)).ToArray();
				await connection.WritePackets(await Task.WhenAll(missingCollaborators.Select(async key =>
					await Requests.CollaboratorData.Create(this, key))));
				// Wait for the answers
				foreach (var key in missingCollaborators)
				{
					var collaboratorDataResponse = await connection.ReadPacket() as Responses.CollaboratorData;
					if (collaboratorDataResponse == null)
						throw new LagerException("Got no collaborator data as response");
					var collaborator = await collaboratorDataResponse.GetCollaborator(this);
					if (collaborator.Key != key)
						throw new LagerException("Got the wrong collaborator data as response");

					// Add the collaborator
					await AddCollaborator(collaborator);
				}

				// Download new bundles
				statusUpdate?.Invoke(NetworkStatus.BundlesRequest);
				int requestedBundles = 0;
				var packets = new List<CommunicationPackets.CommunicationPacket>();
				// Request 100 packets at once
				var currentPacketRequest = new List<Tuple<Collaborator, int>>();
				for (int i = 0; i < Status.BundleCount.Count; i++)
				{
					var collaborator = Collaborators[Status.BundleCount[i].Item1];
					for (int bundleId = Status.GetBundleCount(collaborator);
						 bundleId < Remote.Status.GetBundleCount(collaborator);
						 bundleId++)
					{
						requestedBundles++;
						currentPacketRequest.Add(new Tuple<Collaborator, int>(collaborator, bundleId));
						if (currentPacketRequest.Count == 100)
						{
							packets.Add(await Requests.Bundles.Create(this, currentPacketRequest));
							currentPacketRequest.Clear();
						}
					}
				}
				if (currentPacketRequest.Any())
					packets.Add(await Requests.Bundles.Create(this, currentPacketRequest));
				await connection.WritePackets(packets);
				// Wait for the bundles
				statusUpdate?.Invoke(NetworkStatus.DownloadBundles);
				for (int i = 0; i < requestedBundles; i++)
				{
					var bundleResponse = await connection.ReadPacket() as Responses.Bundle;
					if (bundleResponse == null)
						throw new LagerException("Got no bundle as response");
					await bundleResponse.ReadBundle(this);
				}

				// Upload new bundles
				statusUpdate?.Invoke(NetworkStatus.UploadBundles);
				packets.Clear();
				for (int bundleId = Remote.Status.GetBundleCount(OwnCollaborator);
					 bundleId < Status.GetBundleCount(OwnCollaborator);
					 bundleId++)
				{
					packets.Add(await Requests.UploadBundle.Create(this, OwnCollaborator.Bundles[bundleId]));
				}
				await connection.WritePackets(packets);
				// Check the response
				for (int i = 0; i < packets.Count; i++)
				{
					var uploadResponse = await connection.ReadPacket() as Responses.Status;
					if (uploadResponse == null)
						throw new LagerException("Got no status as response");
					if (!uploadResponse.GetSuccess())
						throw new LagerException("Uploading a bundle failed");
				}

				// Reload the history
				Reset();
				await ApplyHistory();
				statusUpdate?.Invoke(NetworkStatus.Ready);
			}
			finally
			{
				if (connection != null)
					await connection.Close();
			}
		}

		/// <summary>
		/// Upload this lager to the server that is set in the LagerManager.
		/// </summary>
		public async Task Upload(Action<NetworkStatus> statusUpdate)
		{
			// Open a connection
			statusUpdate?.Invoke(NetworkStatus.Connecting);
			INetworkConnection connection = null;
			try
			{
				connection = await Manager.NetworkClient.OpenConnection(
					ClientManager.Settings.ServerAddress, LagerManager.PORT);

				// Create the lager
				statusUpdate?.Invoke(NetworkStatus.AddLager);
				await connection.WritePacket(new Requests.AddLager(this));
				var addLagerResponse = await connection.ReadPacket() as Responses.AddLager;
				if (addLagerResponse == null)
					throw new LagerException("Got no lager id as response");
				Remote = new LagerRemote(addLagerResponse.GetRemoteId());

				// Register our own collaborator
				statusUpdate?.Invoke(NetworkStatus.RegisterCollaborator);
				await connection.WritePacket(await Requests.Register.Create(this));
				var packet = await connection.ReadPacket() as Responses.Register;
				await connection.Close();
				if (packet == null)
					throw new LagerException("Got no register packet as response");
				int collaboratorId = packet.GetCollaboratorId();
				// Temporarily add our own collaborator to the remote status with the id we obtained from the server
				for (int i = 0; i < collaboratorId; i++)
					Remote.Status.AddBundleCount(null);
				Remote.Status.AddBundleCount(new Tuple<KeyPair, int>(OwnCollaborator.Key, 0));
			}
			finally
			{
				if (connection != null)
					await connection.Close();
			}
			await Save();
			await Synchronise(statusUpdate);
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
				return (await Task.WhenAll(col.Bundles.Select(
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
			int maxBundleId = OwnCollaborator.Bundles.Count - 1;
			if (OwnCollaborator.Bundles.Any() &&
			    (Remote == null || Remote.Status.BundleCount.First(c => c.Item1 == OwnCollaborator.Key).Item2 < maxBundleId) &&
				OwnCollaborator.Bundles[maxBundleId].Size < DataPacketBundle.MAX_PACKET_SIZE)
				bundle = OwnCollaborator.Bundles[maxBundleId];
			else
			{
				bundle = new DataPacketBundle();
				OwnCollaborator.AddBundle(bundle);
				Status.UpdateBundleCount(OwnCollaborator);
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
				throw new LagerException("A member with this id exists already.");
			members.Add(member);
		}

		public void RemoveMember(Member member)
		{
			if (!members.Remove(member))
				throw new LagerException("A member with this id wasn't found for deletion.");
		}

		public void AddTent(Tent tent)
		{
			if (Tents.Any(t => t.Id == tent.Id))
				throw new LagerException("A tent with this id exists already.");
			tents.Add(tent);
		}

		public void RemoveTent(Tent tent)
		{
			if (!tents.Remove(tent))
				throw new LagerException("A tent with this id wasn't found for deletion.");
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
			await Data.Serialise();
			await base.Write(output, serialiser, context);
		}

		public override async Task Read(BinaryReader input,
			Serialiser<LagerSerialisationContext> serialiser,
			LagerSerialisationContext context)
		{
			await base.Read(input, serialiser, context);
			await Data.Decrypt(password);
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

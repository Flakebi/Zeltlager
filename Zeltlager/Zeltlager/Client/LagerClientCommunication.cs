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

	public static class InitStatusHelper
	{
		public static string GetMessage(this LagerClient.InitStatus status)
		{
			switch (status)
			{
				case LagerClient.InitStatus.CreateSymmetricKey:
					return "Lagerschlüssel erstellen";
				case LagerClient.InitStatus.CreateGameAsymmetricKey:
					return "Lagerzertifikat erstellen";
				case LagerClient.InitStatus.CreateCollaboratorAsymmetricKey:
					return "Persönliches Zertifikat erstellen";
				case LagerClient.InitStatus.Finished:
					return "Fertig";
				default:
					throw new ArgumentException("LagerClient.InitStatus has an invalid value");
			}
		}
	}

	// This is the saving/network part of the LagerClient, the logic part can be
	// found in LagerClient.cs.
	public partial class LagerClient : LagerBase, ISerialisable<LagerSerialisationContext>, ISerialisable<LagerClientSerialisationContext>, ISearchable
	{
		public enum InitStatus
		{
			CreateSymmetricKey,
			CreateGameAsymmetricKey,
			CreateCollaboratorAsymmetricKey,
			Finished
		}

		const string CLIENT_LAGER_FILE = "client.data";

		public Serialiser<LagerClientSerialisationContext> ClientSerialiser { get; private set; }
			= new Serialiser<LagerClientSerialisationContext>();

		/// <summary>
		/// The collaborator that we are.
		/// </summary>
		public Collaborator OwnCollaborator { get; private set; }

		/// <summary>
		/// The password supplied by the user and used to generate the shared keys.
		/// </summary>
		string password;

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

			statusUpdate?.Invoke(InitStatus.Finished);
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

		public override void Unload()
		{
			base.Unload();
			// Unload all data
			Reset();
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
				statusUpdate?.Invoke(NetworkStatus.Finished);
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
			// We need to apply the whole history again if the packet has a higher priority
			if (packet.Priority != 0)
			{
				Reset();
				if (!await ApplyHistory())
					throw new LagerException("Error applying the packet");
			}
			else
				await packet.Deserialise(ClientSerialiser, context);
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

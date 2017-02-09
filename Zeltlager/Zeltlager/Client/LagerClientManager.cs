using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
	using Cryptography;
	using Network;
	using Requests = CommunicationPackets.Requests;
	using Responses = CommunicationPackets.Responses;

	public class LagerClientManager : LagerManager
	{
		public ClientSettings Settings { get; private set; }

		public LagerClientManager(IIoProvider io) : base(io)
		{
			Settings = new ClientSettings();
		}

		public override async Task Load()
		{
			try
			{
				await Settings.Load(ioProvider);
			}
			catch (Exception e)
			{
				await Log.Exception("Loading settings", e);
			}
			await base.Load();
		}

		protected override async Task<LagerBase> LoadLager(int id)
		{
			IIoProvider io = new RootedIoProvider(ioProvider, id.ToString());
			LagerBase lager = new LagerClient(this, io, id);
			await lager.Load();
			return lager;
		}

		/// <summary>
		/// Create a new lager and save it.
		/// </summary>
		/// <returns>The created lager.</returns>
		/// <param name="name">The name of the new lager.</param>
		/// <param name="password">The password of the new lager.</param>
		/// <param name="statusUpdate">A function that receivs status updates.</param>
		public async Task<LagerClient> CreateLager(string name, string password,
			Action<LagerClient.InitStatus> statusUpdate)
		{
			int id = GetUnusedLagerId();
			IIoProvider io = new RootedIoProvider(ioProvider, id.ToString());
			LagerClient lager = new LagerClient(this, io, id);
			await lager.InitLocal(name, password, statusUpdate);
			// Save the lager
			await lager.Save();

			// Store lager as last used lager
			lagers.Add(id, lager);
			Settings.LastLager = id;
			await Settings.Save(ioProvider);
			return lager;
		}

		public async Task<LagerClient> DownloadLager(int serverId, LagerData data, string password,
			Action<LagerClient.InitStatus> initStatusUpdate, Action<NetworkStatus> networkStatusUpdate)
		{
			int id = GetUnusedLagerId();
			IIoProvider io = new RootedIoProvider(ioProvider, id.ToString());
			LagerClient lager = new LagerClient(this, io, id);
			await lager.InitFromServer(serverId, data, password, initStatusUpdate);
			// Save the lager
			await lager.Save();

			// Register ourself as collaborator
			networkStatusUpdate?.Invoke(NetworkStatus.Connecting);
			INetworkConnection connection = null;
			try
			{
				connection = await NetworkClient.OpenConnection(Settings.ServerAddress, PORT);
				networkStatusUpdate?.Invoke(NetworkStatus.RegisterCollaborator);
				await connection.WritePacket(await Requests.Register.Create(lager));
				var packet = await connection.ReadPacket() as Responses.Register;
				await connection.Close();
				if (packet == null)
					throw new LagerException("Got no register packet as response");
				int collaboratorId = packet.GetCollaboratorId();
				// Temporarily add our own collaborator to the remote status with the id we obtained from the server
				for (int i = 0; i < collaboratorId; i++)
					lager.Remote.Status.AddBundleCount(null);
				lager.Remote.Status.AddBundleCount(new Tuple<KeyPair, int>(lager.OwnCollaborator.Key, 0));
			}
			finally
			{
				if (connection != null)
					await connection.Close();
			}
			// Synchronize the lager
			await lager.Synchronise(networkStatusUpdate);

			// Store lager as last used lager
			lagers.Add(id, lager);
			Settings.LastLager = id;
			await Settings.Save(ioProvider);
			return lager;
		}

		/// <summary>
		/// Get the list of lagers from the remote host.
		/// </summary>
		/// <returns>The lager list with server lager id and lager data.</returns>
		/// <param name="statusUpdate">An optional callback for status updates.</param>
		public async Task<Dictionary<int, LagerData>> RemoteListLagers(Action<NetworkStatus> statusUpdate)
		{
			statusUpdate?.Invoke(NetworkStatus.Connecting);
			INetworkConnection connection = null;
			try
			{
				connection = await NetworkClient.OpenConnection(Settings.ServerAddress, PORT);
				statusUpdate?.Invoke(NetworkStatus.ListLagers);
				await connection.WritePacket(new Requests.ListLagers());
				var packet = (Responses.ListLagers)await connection.ReadPacket();
				await connection.Close();
				var result = packet.GetLagerData();
				statusUpdate?.Invoke(NetworkStatus.Finished);
				return result;
			}
			finally
			{
				if (connection != null)
					await connection.Close();
			}
		}
	}
}

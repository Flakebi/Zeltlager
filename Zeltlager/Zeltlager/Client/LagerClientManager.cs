using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
	using Requests = CommunicationPackets.Requests;
	using Responses = CommunicationPackets.Responses;

	public class LagerClientManager : LagerManager
    {
		public enum NetworkStatus
		{
			Connecting,
			ListLagers,
			RegisterCollaborator,
			Ready
		}

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
            int id = Lagers.Count;
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
			int id = Lagers.Count;
			IIoProvider io = new RootedIoProvider(ioProvider, id.ToString());
			LagerClient lager = new LagerClient(this, io, id);
			await lager.InitFromServer(serverId, data, password, initStatusUpdate);
			// Save the lager
			await lager.Save();

			// Register ourself as collaborator
			{
				networkStatusUpdate?.Invoke(NetworkStatus.Connecting);
				var connection = await NetworkClient.OpenConnection(Settings.ServerAddress, PORT);
				networkStatusUpdate?.Invoke(NetworkStatus.RegisterCollaborator);
				await connection.WritePacket(await Requests.Register.Create(lager));
				var packet = (Responses.Status)await connection.ReadPacket();
				await connection.Close();
				if (!packet.GetSuccess())
					throw new LagerException("Failed to register our collaborator for the lager");
				networkStatusUpdate?.Invoke(NetworkStatus.Ready);
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
		/// <returns>The lager list with server id and lager data.</returns>
		/// <param name="statusUpdate">An optional callback for status updates.</param>
		public async Task<Dictionary<int, LagerData>> RemoteListLagers(Action<NetworkStatus> statusUpdate)
		{
			statusUpdate?.Invoke(NetworkStatus.Connecting);
			var connection = await NetworkClient.OpenConnection(Settings.ServerAddress, PORT);
			statusUpdate?.Invoke(NetworkStatus.ListLagers);
			await connection.WritePacket(new Requests.ListLagers());
			var packet = (Responses.ListLagers)await connection.ReadPacket();
			await connection.Close();
			var result = packet.GetLagerData();
			statusUpdate?.Invoke(NetworkStatus.Ready);
			return result;
		}
    }
}

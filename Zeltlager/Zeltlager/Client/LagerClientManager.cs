using System;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
    public class LagerClientManager : LagerManager
    {
        public ClientSettings Settings { get; private set; }
        
        public LagerClientManager(IIoProvider io) : base(io)
        {
            Settings = new ClientSettings();
        }

        public override async Task Load()
        {
            await Settings.Load(ioProvider);
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

		public async Task<LagerClient> AddLager(int serverId, LagerData data, string password,
			Action<LagerClient.InitStatus> statusUpdate)
		{
			int id = Lagers.Count;
			IIoProvider io = new RootedIoProvider(ioProvider, id.ToString());
			LagerClient lager = new LagerClient(this, io, id);
			await lager.InitFromServer(serverId, data, password, statusUpdate);
			// Save the lager
			await lager.Save();

			// Store lager as last used lager
			lagers.Add(id, lager);
			Settings.LastLager = id;
			await Settings.Save(ioProvider);
			return lager;
		}
    }
}

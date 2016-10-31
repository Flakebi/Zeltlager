using System;
using System.Threading.Tasks;

namespace Zeltlager.Client
{
    public class ClientLagerManager : LagerManager
    {
        public ClientSettings Settings { get; private set; }
        
        public ClientLagerManager(IIoProvider io) : base(io)
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

        public async Task CreateLager(string name, string password,
            Action<LagerClient.InitStatus> statusUpdate)
        {
            int id = Lagers.Count;
            IIoProvider io = new RootedIoProvider(ioProvider, id.ToString());
            LagerClient lager = new LagerClient(this, io, id);
            await lager.Init(name, password, statusUpdate);
            // Save tha lager
            await lager.Save();

            // Store lager as last used lager
            lagers[id] = lager;
            Settings.LastLager = id;
            await Settings.Save(ioProvider);
        }
    }
}

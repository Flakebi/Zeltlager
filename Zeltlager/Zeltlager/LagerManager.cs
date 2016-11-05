using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Cryptography;

	public class LagerManager
	{
		public static bool IsClient { get; set; }
		public static ICryptoProvider CryptoProvider { get; set; }
		public static Log Log { get; set; }

		static LagerManager()
		{
			CryptoProvider = new BCCryptoProvider();
		}

		protected IIoProvider ioProvider;

        protected Dictionary<int, LagerBase> lagers = new Dictionary<int, LagerBase>();
		public IReadOnlyDictionary<int, LagerBase> Lagers => lagers;

        public LagerManager(IIoProvider io)
        {
            Log = new Log(io);
            ioProvider = io;
        }

		/// <summary>
		/// Load all lagers.
		/// </summary>
        public virtual async Task Load()
		{
			// Search folders for lagers
			var folders = await ioProvider.ListContents("");
			for (int i = 0; folders.Contains(new Tuple<string, FileType>(i.ToString(), FileType.Folder)); i++)
			{
				try
				{
					LagerBase lager = await LoadLager(i);
					lagers.Add(i, lager);
				} catch (Exception e)
				{
					await Log.Exception("Loading lagers", e);
				}
			}
		}

        protected virtual async Task<LagerBase> LoadLager(int id)
		{
			IIoProvider io = new RootedIoProvider(ioProvider, id.ToString());
			LagerBase lager = new LagerBase(this, io, id);
			await lager.Load();
			return lager;
		}
	}
}

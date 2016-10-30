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
			Log = new Log();
		}

		protected IIoProvider ioProvider;

		Dictionary<int, LagerBase> lagers = new Dictionary<int, LagerBase>();
		public IReadOnlyDictionary<int, LagerBase> Lagers => lagers;

		/// <summary>
		/// Load all lagers.
		/// </summary>
		/// <returns></returns>
		public async Task Load()
		{
			//TODO
			// Search folders for lagers
			var folders = await ioProvider.ListContents("");
			int lagerCount = 0;
			while (folders.Contains(new Tuple<string, FileType>(lagerCount.ToString(), FileType.Folder)))
				lagerCount++;

			for (int i = 0; i < lagerCount; i++)
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

		protected async Task<LagerBase> LoadLager(int id)
		{
			IIoProvider io = new RootedIoProvider(ioProvider, id.ToString());
			LagerBase lager;
			if (IsClient)
				lager = new Client.LagerClient(this, io, id);
			else
				lager = new LagerBase(this, io, id);
			await lager.Load();
			return lager;
		}
	}
}

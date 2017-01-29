using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Zeltlager
{
	using Cryptography;
	using Network;
	using Requests = CommunicationPackets.Requests;

	public class LagerManager
	{
		/// <summary>
		/// 5, 7, 9
		/// </summary>
		public const ushort PORT = 57911;

		public static bool IsClient { get; set; }
		public static ICryptoProvider CryptoProvider { get; set; }
		public static Log Log { get; set; }

		static LagerManager()
		{
			CryptoProvider = new BCCryptoProvider();
		}

		protected IIoProvider ioProvider;
		public IIoProvider IoProvider => ioProvider;

		/// <summary>
		/// The lagers with their respective ids.
		/// </summary>
        protected Dictionary<int, LagerBase> lagers = new Dictionary<int, LagerBase>();
		public IReadOnlyDictionary<int, LagerBase> Lagers => lagers;

		/// <summary>
		/// The client that is used to create network connections.
		/// </summary>
		public INetworkClient NetworkClient { get; set; }
		INetworkServer server;
		/// <summary>
		/// The server that listens for new connections if this object is not null.
		/// </summary>
		public INetworkServer NetworkServer
		{
			get
			{
				return server;
			}

			set
			{
				server = value;
				// Start the server if it exists
				server?.SetOnAcceptConnection(OnNetworkConnection);
				server?.Start(PORT);
			}
		}

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
			foreach (var folder in folders.Where(f => f.Item2 == FileType.Folder).Select(f => f.Item1))
			{
				int i;
				if (int.TryParse(folder, out i))
				{
					try
					{
						LagerBase lager = await LoadLager(i);
						lagers.Add(i, lager);
						await Log.Info("Loading lagers", "Added lager " + i);
					} catch (Exception e)
					{
						await Log.Exception("Loading lagers", e);
					}
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

		/// <summary>
		/// Add a lager from lager data.
		/// </summary>
		/// <returns>The created lager.</returns>
		/// <param name="data">The data for the new lager.</param>
		public async Task<LagerBase> AddLager(LagerData data)
		{
			// Check if the same lager exists already
			foreach (LagerBase l in lagers.Values)
			{
				if (data.Data.SequenceEqual(l.Data.Data))
					return l;
			}
			int id = GetUnusedLagerId();
			IIoProvider io = new RootedIoProvider(ioProvider, id.ToString());
			LagerBase lager = new LagerBase(this, io, id);
			lager.Data = data;
			await lager.Save();
			lagers.Add(lager.Id, lager);
			return lager;
		}

		/// <summary>
		/// Get the lowest unused id for a lager.
		/// </summary>
		/// <returns>An unused lager id.</returns>
		protected int GetUnusedLagerId()
		{
			// Search the first unused id
			int id;
			for (id = 0; id < lagers.Count; id++)
			{
				if (!lagers.ContainsKey(id))
					break;
			}
			return id;
		}

		async Task OnNetworkConnection(INetworkConnection connection)
		{
			try
			{
				// Handle all requests
				while (!connection.IsClosed)
				{
					var packet = await connection.ReadPacket();
					var request = packet as Requests.CommunicationRequest;
					if (request == null)
						throw new LagerException("Unexpectd communication packet type");

					await request.Apply(connection, this);
				}
			}
			// Ignore if the connection shut down
			catch (EndOfStreamException) {}
			catch (Exception e)
			{
				await Log.Exception("Network connection", e);
			}
			finally
			{
				try
				{
					await connection.Close();
				}
				// Ignore if closing the connection failed
				catch (Exception e)
				{
					await Log.Exception("Network connection closing", e);
				}
				connection.Dispose();
			}
		}
	}
}

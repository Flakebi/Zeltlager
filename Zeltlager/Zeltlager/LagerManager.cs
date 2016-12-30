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
	using Responses = CommunicationPackets.Responses;

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
			// TODO This could be improved to only load lagers completely on demand.
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
					{
						throw new InvalidOperationException("Unexpectd communication packet type");
					}
					request.Apply(connection, this);
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

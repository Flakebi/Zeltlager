using System;
using System.IO;
using System.Threading.Tasks;

using Sockets.Plugin;
using Sockets.Plugin.Abstractions;

namespace Zeltlager.Network
{
	using CommunicationPackets;

	public class TcpNetworkServer : INetworkServer
	{
		TcpSocketListener socketListener;
		Func<INetworkConnection, Task> onAccept;

		public TcpNetworkServer()
		{
			socketListener = new TcpSocketListener();
			socketListener.ConnectionReceived += (sender, args) =>
				onAccept?.Invoke(new TcpSocketNetworkConnection(args.SocketClient));
		}

		public Task Start(ushort port)
		{
			return socketListener.StartListeningAsync(port);
		}

		public void SetOnAcceptConnection(Func<INetworkConnection, Task> onAccept)
		{
			this.onAccept = onAccept;
		}
	}

	public class TcpNetworkClient : INetworkClient
	{
		public async Task<INetworkConnection> OpenConnection(string address, ushort port)
		{
			var client = new TcpSocketClient();
			await client.ConnectAsync(address, port);
			return new TcpSocketNetworkConnection(client);
		}
	}

	class TcpSocketNetworkConnection : INetworkConnection
	{
		readonly ITcpSocketClient socketClient;

		public bool IsClosed => !socketClient.ReadStream.CanRead || !socketClient.WriteStream.CanWrite;

		public TcpSocketNetworkConnection(ITcpSocketClient socketClient)
		{
			this.socketClient = socketClient;
		}

		public void Dispose()
		{
			socketClient.Dispose();
		}

		public string GetRemoteAddress()
		{
			return socketClient.RemoteAddress;
		}

		public ushort GetRemotePort()
		{
			return (ushort)socketClient.RemotePort;
		}

		public async Task<CommunicationPacket> ReadPacket()
		{
			int length = (await socketClient.ReadStream.ReadAsyncSafe(sizeof(int))).ToInt(0);
			return CommunicationPacket.ReadPacket(await socketClient.ReadStream.ReadAsyncSafe(length));
		}

		public Task WritePacket(CommunicationPacket packet)
		{
			return WritePackets(new CommunicationPacket[] { packet });
		}

		public async Task WritePackets(CommunicationPacket[] packets)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter writer = new BinaryWriter(mem))
			{
				foreach (var packet in packets)
				{
					MemoryStream tmp = new MemoryStream();
					using (BinaryWriter tmpWriter = new BinaryWriter(tmp))
						packet.WritePacket(tmpWriter);
					byte[] data = tmp.ToArray();
					writer.Write(data.Length.ToBytes());
					writer.Write(data);
				}
			}
			byte[] result = mem.ToArray();
			await socketClient.WriteStream.WriteAsync(result, 0, result.Length);
			await socketClient.WriteStream.FlushAsync();
		}

		public Task Close()
		{
			return socketClient.DisconnectAsync();
		}
	}
}

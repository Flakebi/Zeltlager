using System;
using System.Threading.Tasks;

namespace Zeltlager.Network
{
	using CommunicationPackets;

	public interface INetworkClient
	{
		Task<INetworkConnection> OpenConnection(string address, ushort port);
	}

	public interface INetworkServer
	{
		Task Start(ushort port);
		void SetOnAcceptConnection(Func<INetworkConnection, Task> onAccept);
	}

	public interface INetworkConnection : IDisposable
	{
		string GetRemoteAddress();
		ushort GetRemotePort();

		Task WritePacket(CommunicationPacket packet);
		/// <summary>
		/// More efficient for multiple packets because they can be bundled.
		/// </summary>
		/// <param name="packets">A list of packets that should be written to this connection.</param>
		Task WritePackets(CommunicationPacket[] packets);
		Task<CommunicationPacket> ReadPacket();
		Task Close();
	}
}

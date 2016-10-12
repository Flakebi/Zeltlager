using System;

namespace Zeltlager.Network
{
	using CommunicationPackets;

	public interface INetworkClient
	{
		INetworkConnection OpenConnection(byte[] address, ushort port);
	}

	public interface INetworkServer
	{
		INetworkConnection AcceptConnection();
	}

	public interface INetworkConnection : IDisposable
	{
		byte[] GetAddress();
		ushort GetPort();

		void WritePacket(CommunicationPacket packet);
		/// <summary>
		/// More efficient for multiple packets because they can be bundled.
		/// </summary>
		/// <param name="packets">A list of packets that should be written to this connection.</param>
		void WritePackets(CommunicationPacket[] packets);
		CommunicationPacket ReadPacket();
	}
}

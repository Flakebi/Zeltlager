using System;

namespace Zeltlager.Network
{
	using CommunicationPackets;

	public interface INetworkProvider
	{
		INetworkConnection OpenConnection(byte[] address, ushort port);
	}

	public interface INetworkConnection : IDisposable
	{
		void WritePacket(CommunicationPacket packet);
		void WritePackets(CommunicationPacket[] packet);
		CommunicationPacket ReadPacket();
	}
}

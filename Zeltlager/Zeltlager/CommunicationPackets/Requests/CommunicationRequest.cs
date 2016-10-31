using System;
using System.IO;

namespace Zeltlager.CommunicationPackets.Requests
{
    using Network;
    
	public abstract class CommunicationRequest : CommunicationPacket
	{
		/// <summary>
		/// Apply the request to a lager and send a response.
		/// </summary>
		/// <param name="lagerServer"></param>
		public abstract void Apply(INetworkConnection connection, LagerManager manager);
	}
}

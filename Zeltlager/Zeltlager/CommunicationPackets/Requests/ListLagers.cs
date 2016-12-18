using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zeltlager.Network;

namespace Zeltlager.CommunicationPackets.Requests
{
    public class ListLagers : CommunicationRequest
    {
		public ListLagers()
		{
			Data = new byte[0];
		}

		public override void Apply(INetworkConnection connection, LagerManager manager)
		{
			// Create a response
			connection.WritePacket(new Responses.ListLagers(manager));
		}
	}
}

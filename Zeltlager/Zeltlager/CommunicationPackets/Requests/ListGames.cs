using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zeltlager.Network;

namespace Zeltlager.CommunicationPackets.Requests
{
    public class ListGames : CommunicationRequest
    {
		public ListGames() { }

		public override void Apply(INetworkConnection connection, ILagerServer lagerServer)
		{
			// Create a response
		}
	}
}

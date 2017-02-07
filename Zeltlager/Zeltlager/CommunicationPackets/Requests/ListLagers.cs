using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Requests
{
	using Network;

	public class ListLagers : CommunicationRequest
	{
		public ListLagers()
		{
			Data = new byte[0];
		}

		public override Task Apply(INetworkConnection connection, LagerManager manager)
		{
			// Create a response
			return connection.WritePacket(new Responses.ListLagers(manager));
		}
	}
}

using System.Threading.Tasks;

namespace Zeltlager.CommunicationPackets.Requests
{
	using Network;

	public abstract class CommunicationRequest : CommunicationPacket
	{
		/// <summary>
		/// Apply the request to a lager and send a response.
		/// </summary>
		/// <param name="connection">The connection where this request was read.</param>
		/// <param name="manager">The lager manager.</param>
		public abstract Task Apply(INetworkConnection connection, LagerManager manager);
	}
}

using System.IO;
using System.Threading.Tasks;
using Zeltlager.Client;

namespace Zeltlager.DataPackets
{
	
	/// <summary>
	/// Ignore a specific packet.
	/// </summary>
	public class RevertPacket : DataPacket
	{
		// Apply RevertPackets before normal packets.
		public override int Priority => -1;

		public PacketId PacketId { get; set; }

		protected RevertPacket() { }

		/// <summary>
		/// Creates a new RevertPacket.
		/// </summary>
		/// <param name="packet">The packet that should be reverted.</param>
		public RevertPacket(PacketId packet)
		{
			PacketId = packet;
		}

		public override async Task Deserialise(LagerClient lager)
		{
			contentString = "Revert for " + PacketId;
			for (int i = 0; i < lager.Collaborators.Packets.Count; i++)
				{
					if (context.Packets[i].Id == packet)
					{
						context.Packets.RemoveAt(i);
						break;
					}
				}

		}

		public override int CompareTo(DataPacket other)
		{
			if (other is RevertPacket)
				// The newest revert packet is the most important
				return other.Timestamp.CompareTo(Timestamp);
			return base.CompareTo(other);
		}
	}
}

using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Serialisation;

	/// <summary>
	/// Ignore a specific packet.
	/// </summary>
	public class RevertPacket : DataPacket
	{
		// Apply RevertPackets before normal packets.
		public override int Priority => -1;

		protected RevertPacket() { }

		/// <summary>
		/// Creates a new RevertPacket.
		/// </summary>
		/// <param name="packet">The packet that should be reverted.</param>
		public RevertPacket(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, PacketId packet)
		{
			var mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
				serialiser.Write(output, context, packet);
			Data = mem.ToArray();
		}

		public override async Task Deserialise(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context)
		{
			using (BinaryReader input = new BinaryReader(new MemoryStream(Data)))
			{
				PacketId packet = await serialiser.Read(input, context, new PacketId(context.PacketId.Creator));
				contentString = packet.ToString();
				for (int i = 0; i < context.Packets.Count; i++)
				{
					if (context.Packets[i].Id == packet)
					{
						context.Packets.RemoveAt(i);
						break;
					}
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

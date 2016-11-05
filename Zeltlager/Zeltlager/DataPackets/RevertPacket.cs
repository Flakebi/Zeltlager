using System.IO;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	using Serialisation;

	public class RevertPacket : DataPacket
	{
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
			{
				serialiser.Write(output, context, packet);
			}			
		}

		public override async Task Deserialise(Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context)
		{
			using (BinaryReader input = new BinaryReader(new MemoryStream(Data)))
			{
				PacketId packet = await serialiser.Read(input, context, new PacketId(context.PacketId.Creator));
				//TODO Revert the packet and all packets that depend on it
			}
		}
	}
}

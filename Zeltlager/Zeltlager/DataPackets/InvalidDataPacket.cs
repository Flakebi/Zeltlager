using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
    using Serialisation;

    /// <summary>
    /// Represents an invalid packet. It contains only the raw data.
    /// </summary>
    public class InvalidDataPacket : DataPacket
	{
		public InvalidDataPacket(byte[] data)
		{
			Data = data;
		}

        public override Task Deserialise(
            Serialiser<LagerClientSerialisationContext> serialiser,
            LagerClientSerialisationContext context)
		{
            // Just do nothing here
            return Task.WhenAll();
		}
	}
}

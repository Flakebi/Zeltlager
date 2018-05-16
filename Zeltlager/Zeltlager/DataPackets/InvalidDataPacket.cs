using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	
	/// <summary>
	/// Represents an invalid packet. It contains only the raw data.
	/// </summary>
	public class InvalidDataPacket : DataPacket
	{
		public InvalidDataPacket(byte[] data)
		{
			Data = data;
		}

		public override Task Deserialise()
		{
			// Just do nothing here
			return Task.WhenAll();
		}
	}
}

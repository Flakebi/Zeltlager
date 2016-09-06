using System.IO;

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

		public override void Deserialise(Lager lager)
		{
			// Just do nothing here
		}
	}
}

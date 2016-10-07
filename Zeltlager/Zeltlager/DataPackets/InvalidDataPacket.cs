using System.IO;

namespace Zeltlager.DataPackets
{
	using Client;

	/// <summary>
	/// Represents an invalid packet. It contains only the raw data.
	/// </summary>
	public class InvalidDataPacket : DataPacket
	{
		public InvalidDataPacket(byte[] data)
		{
			Data = data;
		}

		public override void Deserialise(LagerClient lager)
		{
			// Just do nothing here
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	/// <summary>
	/// Represents an invalid packet. It contains only the raw data.
	/// </summary>
	class InvalidDataPacket : DataPacket
	{
		byte[] data;

		public InvalidDataPacket(byte[] data)
		{
			this.data = data;
		}

		public override bool Apply(Lager lager)
		{
			// Just do nothing here
			return true;
		}

		protected override void WritePacketData(BinaryWriter output)
		{
			output.Write(data);
		}
	}
}

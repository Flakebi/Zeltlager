using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	class GeneralDataPacket : DataPacket
	{
		public enum PacketType : byte
		{
			AddMember,
			DeleteMember,
		}

		public GeneralDataPacket(BinaryReader input, PacketType packetType, Lager zeltlager)
		{

		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	class DeleteTentPacket : DataPacket
	{
		byte number;

		public DeleteTentPacket(BinaryReader input, Lager lager)
		{
			number = input.ReadByte();
		}

		public DeleteTentPacket(Tent tent)
		{
			number = tent.Number;
		}

		protected override void WritePacketData(BinaryWriter output)
		{
			output.Write(number);
		}

		public override bool Apply(Lager lager)
		{
			Tent tent = lager.Tents.FirstOrDefault(t => t.Number == number);
			if (tent == null)
				return false;
			lager.RemoveTent(tent);
			return true;
		}
	}
}

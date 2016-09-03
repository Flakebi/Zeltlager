using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	public class DeleteTentPacket : DataPacket
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

		public override void Apply(Lager lager)
		{
			Tent tent = lager.Tents.First(t => t.Number == number);
			lager.RemoveTent(tent);
		}
	}
}

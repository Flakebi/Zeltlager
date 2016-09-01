using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	class DeleteSupervisorFromTentPacket : DataPacket
	{
		ushort id;
		byte number;

		public DeleteSupervisorFromTentPacket(BinaryReader input, Lager lager)
		{
			id = input.ReadUInt16();
			number = input.ReadByte();
		}

		public DeleteSupervisorFromTentPacket(Member supervisor, Tent tent)
		{
			id = supervisor.Id;
			number = tent.Number;
		}

		protected override void WritePacketData(BinaryWriter output)
		{
			output.Write(id);
			output.Write(number);
		}

		public override bool Apply(Lager lager)
		{
			Member supervisor = lager.Members.FirstOrDefault(m => m.Id == id);
			if (supervisor == null)
				return false;

			Tent tent = lager.Tents.FirstOrDefault(t => t.Number == number);
			if (tent == null)
				return false;

			return tent.RemoveSupervisor(supervisor);
		}
	}
}

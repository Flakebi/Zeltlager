using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	class AddSupervisorToTentPacket : DataPacket
	{
		ushort id;
		byte number;

		public AddSupervisorToTentPacket(BinaryReader input, Lager lager)
		{
			id = input.ReadUInt16();
			number = input.ReadByte();
		}

		public AddSupervisorToTentPacket(Member supervisor, Tent tent)
		{
			id = supervisor.Id;
			number = tent.Number;
		}

		protected override void WritePacketData(BinaryWriter output)
		{
			output.Write(id);
			output.Write(number);
		}

		public override void Apply(Lager lager)
		{
			Member supervisor = lager.Members.First(m => m.Id == id);
			Tent tent = lager.Tents.First(t => t.Number == number);
			if (tent.Supervisors.Contains(supervisor))
				throw new InvalidOperationException("The tent already contains the specified supervisor.");
			tent.Supervisors.Add(supervisor);
		}
	}
}

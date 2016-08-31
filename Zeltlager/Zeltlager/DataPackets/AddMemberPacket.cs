using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	class AddMemberPacket : DataPacket
	{
		ushort id;
		string name;
		bool supervisor;
		byte tentNumber;

		public AddMemberPacket(BinaryReader input, Lager lager)
		{
			id = input.ReadUInt16();
			name = input.ReadString();
			supervisor = input.ReadBoolean();
			tentNumber = input.ReadByte();
		}

		public AddMemberPacket(Member member)
		{
			id = member.Id;
			name = member.Name;
			supervisor = member.Supervisor;
			tentNumber = member.Tent.Number;
		}

		protected override void WritePacketData(BinaryWriter output)
		{
			output.Write(id);
			output.Write(name);
			output.Write(supervisor);
			output.Write(tentNumber);
		}

		public override bool Apply(Lager lager)
		{
			Tent tent = lager.Tents.FirstOrDefault(t => t.Number == tentNumber);
			Member member = new Member(id, name, tent, supervisor);
			lager.AddMember(member);
			return true;
		}
	}
}

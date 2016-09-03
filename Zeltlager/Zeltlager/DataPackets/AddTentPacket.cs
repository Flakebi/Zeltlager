using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	public class AddTentPacket : DataPacket
	{
		byte number;
		string name;
		ushort[] supervisors;

		public AddTentPacket(BinaryReader input, Lager lager)
		{
			number = input.ReadByte();
			name = input.ReadString();
			ushort length = input.ReadUInt16();
			supervisors = new ushort[length];
			for (int i = 0; i < length; i++)
				supervisors[i] = input.ReadUInt16();

		}

		public AddTentPacket(Tent tent)
		{
			number = tent.Number;
			name = tent.Name;
			supervisors = tent.Supervisors.Select(s => s.Id).ToArray();
		}

		protected override void WritePacketData(BinaryWriter output)
		{
			output.Write(number);
			output.Write(name);
			output.Write(supervisors.Length);
			for (int i = 0; i < supervisors.Length; i++)
				output.Write(supervisors[i]);
		}

		public override void Apply(Lager lager)
		{
			List<Member> members = supervisors.Select(id => lager.Members.First(m => m.Id == id)).ToList();
			Tent tent = new Tent(number, name, members);
			lager.AddTent(tent);
		}
	}
}

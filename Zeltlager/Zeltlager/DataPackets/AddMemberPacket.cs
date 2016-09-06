﻿using System.IO;
using System.Linq;

namespace Zeltlager.DataPackets
{
	public class AddMemberPacket : DataPacket
	{
		public AddMemberPacket() { }

		public AddMemberPacket(Member member)
		{
			MemoryStream mem = new MemoryStream();
			using (BinaryWriter output = new BinaryWriter(mem))
			{
				output.Write(member.Id);
				output.Write(member.Name);
				output.Write(member.Name);
				output.Write(member.Tent.Number);
				Data = mem.ToArray();
			}
		}

		public override void Deserialise(Lager lager)
		{
			MemoryStream mem = new MemoryStream(Data);
			using (BinaryReader input = new BinaryReader(mem))
			{
				ushort id = input.ReadUInt16();
				string name = input.ReadString();
				bool supervisor = input.ReadBoolean();
				byte tentNumber = input.ReadByte();

				Tent tent = lager.Tents.First(t => t.Number == tentNumber);
				lager.AddMember(new Member(id, name, tent, supervisor));
			}
		}
	}
}

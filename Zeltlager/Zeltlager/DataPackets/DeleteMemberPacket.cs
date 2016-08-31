﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeltlager.DataPackets
{
	class DeleteMemberPacket : DataPacket
	{
		ushort id;

		public DeleteMemberPacket(BinaryReader input, Lager lager)
		{
			id = input.ReadUInt16();
		}

		public DeleteMemberPacket(Member member)
		{
			id = member.Id;
		}

		protected override void WritePacketData(BinaryWriter output)
		{
			output.Write(id);
		}

		public override void Apply(Lager lager)
		{
			Member member = lager.Members.First(m => m.Id == id);
			lager.RemoveMember(member);
		}
	}
}
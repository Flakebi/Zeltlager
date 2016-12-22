﻿using System;
using Zeltlager.Serialisation;
namespace Zeltlager.Competition
{
	public class MemberParticipant : Participant
	{
		public override string Name
		{
			get { return member.Display; }
		}

		[Serialisation(Type = SerialisationType.Reference)]
		Member member;

		public MemberParticipant() { }

		public MemberParticipant(Serialisation.LagerClientSerialisationContext context) : base(context) { }

		public MemberParticipant(DataPackets.PacketId id, Member mem, Competition competition) : base(id, competition)
		{
			member = mem;
		}

		public override Participant Clone()
		{
			return new MemberParticipant(Id, member, competition);
		}
	}
}
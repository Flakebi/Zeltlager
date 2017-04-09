using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using DataPackets;
	using Serialisation;

	public class MemberParticipant : Participant
	{
		public override string Name => Member.Display;

		[Serialisation(Type = SerialisationType.Reference)]
		public Member Member { get; set; }

		static Task<MemberParticipant> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult((MemberParticipant)context.LagerClient.CompetitionHandler.GetParticipantFromId(id));
		}

		public MemberParticipant() { }

		public MemberParticipant(LagerClientSerialisationContext context) : base(context) { }

		public MemberParticipant(PacketId id, Member mem, Competition competition) : base(id, competition)
		{
			Member = mem;
		}

		public override Participant Clone()
		{
			return new MemberParticipant(Id?.Clone(), Member, competition);
		}

		public override bool Equals(Participant other)
		{
			if (other is MemberParticipant)
				return Member.Equals(((MemberParticipant)other).Member);
			return false;
		}
	}
}

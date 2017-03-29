using Zeltlager.Serialisation;
namespace Zeltlager.Competition
{
	public class MemberParticipant : Participant
	{
		public override string Name
		{
			get { return Member.Display; }
		}

		[Serialisation(Type = SerialisationType.Reference)]
		public Member Member { get; private set; }

		public MemberParticipant() { }

		public MemberParticipant(LagerClientSerialisationContext context) : base(context) { }

		public MemberParticipant(DataPackets.PacketId id, Member mem, Competition competition) : base(id, competition)
		{
			Member = mem;
		}

		public override Participant Clone()
		{
			return new MemberParticipant(Id, Member, competition);
		}

		public override bool Equals(Participant other)
		{
			if (other is MemberParticipant)
			{
				return Member.Equals(((MemberParticipant)other).Member);
			}
			return false;
		}
	}
}

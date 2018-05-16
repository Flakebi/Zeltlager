using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using DataPackets;
	
	public class MemberParticipant : Participant
	{
		public override string Name => Member.Display;

		// TODO json reference
		public Member Member { get; set; }

		public MemberParticipant() { }

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

using System;
using Zeltlager.Serialisation;
namespace Zeltlager.Competition
{
	public class TentParticipant : Participant
	{
		public override string Name
		{
			get { return tent.Display; }
		}

		[Serialisation(Type = SerialisationType.Reference)]
		Tent tent;

		public TentParticipant() { }

		public TentParticipant(LagerClientSerialisationContext context) : base(context) { }

		public TentParticipant(DataPackets.PacketId id, Tent tent, Competition competition) : base(id, competition)
		{
			this.tent = tent;
		}

		public override Participant Clone()
		{
			return new TentParticipant(Id, tent, competition);
		}

		public override bool Equals(Participant other)
		{
			if (other is TentParticipant)
			{
				return tent.Equals(((TentParticipant)other).tent);
			}
			return false;
		}
	}
}

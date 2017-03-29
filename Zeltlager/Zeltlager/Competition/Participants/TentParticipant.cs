using System;
using Zeltlager.Serialisation;
namespace Zeltlager.Competition
{
	public class TentParticipant : Participant
	{
		public override string Name
		{
			get { return Tent.Display; }
		}

		[Serialisation(Type = SerialisationType.Reference)]
		public Tent Tent { get; private set; }

		public TentParticipant() { }

		public TentParticipant(LagerClientSerialisationContext context) : base(context) { }

		public TentParticipant(DataPackets.PacketId id, Tent tent, Competition competition) : base(id, competition)
		{
			this.Tent = tent;
		}

		public override Participant Clone()
		{
			return new TentParticipant(Id, Tent, competition);
		}

		public override bool Equals(Participant other)
		{
			if (other is TentParticipant)
			{
				return Tent.Equals(((TentParticipant)other).Tent);
			}
			return false;
		}
	}
}

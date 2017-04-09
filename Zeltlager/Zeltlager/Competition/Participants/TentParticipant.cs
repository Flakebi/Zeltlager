using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using DataPackets;
	using Serialisation;

	public class TentParticipant : Participant
	{
		public override string Name => Tent.Display;

		[Serialisation(Type = SerialisationType.Reference)]
		public Tent Tent { get; set; }

		static Task<TentParticipant> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult((TentParticipant)context.LagerClient.CompetitionHandler.GetParticipantFromId(id));
		}

		public TentParticipant() { }

		public TentParticipant(LagerClientSerialisationContext context) : base(context) { }

		public TentParticipant(PacketId id, Tent tent, Competition competition) : base(id, competition)
		{
			Tent = tent;
		}

		public override Participant Clone()
		{
			return new TentParticipant(Id?.Clone(), Tent, competition);
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

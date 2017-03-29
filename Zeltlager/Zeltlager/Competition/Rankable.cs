using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Competition;
	using DataPackets;
	using Serialisation;

	public abstract class Rankable : Editable<Rankable>
	{
		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		public Ranking Ranking { get; set; }

		static Task<Rankable> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.CompetitionHandler.GetRankableFromPacketId(id));
		}

		public abstract void Add(LagerClientSerialisationContext context);

		public abstract void AddResult(CompetitionResult cr);

		public abstract IReadOnlyList<Participant> GetParticipants();
	}
}

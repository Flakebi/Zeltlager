using System;
using System.Threading.Tasks;
using Zeltlager.DataPackets;
using Zeltlager.Serialisation;
using Zeltlager.Client;
using Zeltlager.Competition;
using System.Collections.Generic;
namespace Zeltlager
{
	public abstract class Rankable : Editable<Rankable>
	{
		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		public Ranking Ranking { get; set; }

		static Task<Rankable> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.CompetitionHandler.GetRankableFromId(id));
		}

		public abstract void Add(LagerClientSerialisationContext context);

		public abstract void AddResult(CompetitionResult cr);

		public abstract IReadOnlyList<Participant> GetParticipants();
	}
}

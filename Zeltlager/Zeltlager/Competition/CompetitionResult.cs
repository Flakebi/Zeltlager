using System;
using Zeltlager.Serialisation;
using Zeltlager.DataPackets;
using Zeltlager.UAM;
using System.Collections.Generic;
namespace Zeltlager.Competition
{
	[Editable("Ergebnis")]
	public class CompetitionResult
	{
		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Editable("Punkte")]
		[Serialisation]
		int? Points { get; set; }

		[Editable("Platzierung")]
		[Serialisation]
		int? Place { get; set; }

		[Editable("Teilnehmer")]
		[Serialisation(Type = SerialisationType.Reference)]
		Participant Participant { get; set; }

		IReadOnlyList<Participant> ParticipantList => Owner.GetParticipants();

		[Serialisation(Type = SerialisationType.Reference)]
		public Rankable Owner { get; set; }

		public string PointsString => Points?.ToString() ?? "default";
		public string PlaceString => Place?.ToString() ?? "default";

		public CompetitionResult() {}

		public CompetitionResult(LagerClientSerialisationContext context) : this() {}

		public CompetitionResult(Rankable owner, Participant p, int? points = null, int? place = null)
		{
			Points = points;
			Place = place;
			Participant = p;
			Owner = owner;
		}

		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			context.LagerClient.CompetitionHandler.AddCompetitionResult(this);
		}
	}
}

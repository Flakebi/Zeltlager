using System;
using Zeltlager.Serialisation;
using Zeltlager.DataPackets;
using Zeltlager.UAM;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	[Editable("Ergebnis")]
	public class CompetitionResult : Editable<CompetitionResult>
	{
		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Editable("Punkte")]
		[Serialisation]
		public int? Points { get; set; }

		[Editable("Platzierung")]
		[Serialisation]
		public int? Place { get; set; }

		[Editable("Teilnehmer")]
		[Serialisation(Type = SerialisationType.Reference)]
		public Participant Participant { get; set; }

		public IReadOnlyList<Participant> ParticipantList => Owner.GetParticipants();

		[Serialisation(Type = SerialisationType.Reference)]
		public Rankable Owner { get; set; }

		public string PointsString => Points?.ToString() ?? "default";
		public string PlaceString => Place?.ToString() ?? "default";

		public CompetitionResult() {}

		public CompetitionResult(LagerClientSerialisationContext context) : this() {}

		public CompetitionResult(PacketId id, Rankable owner, Participant p, int? points = null, int? place = null)
		{
			Id = id;
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

		#region Interface implementation

		public override CompetitionResult Clone()
		{
			return new CompetitionResult(Id, Owner, Participant, Points, Place);
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using DataPackets;
	using Serialisation;
	using UAM;

	[Editable("Ergebnis")]
	public class CompetitionResult : Editable<CompetitionResult>, IComparable<CompetitionResult>
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

		public IReadOnlyList<Participant> ParticipantList 
		{ 
			get 
			{
				return Owner.GetParticipants()
					        .Except(Owner.Ranking.Results
					                .Where(cr => cr.Points.HasValue || cr.Place.HasValue)
					                .Select(cr => cr.Participant)).ToList();
			}
		}

		[Serialisation(Type = SerialisationType.Reference)]
		public Rankable Owner { get; set; }

		public string PointsString => Points?.ToString() ?? "-";
		public string PlaceString => Place?.ToString() ?? "-";

		protected static Task<CompetitionResult> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.CompetitionHandler.GetCompetitionResultFromPacketId(id));
		}

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
			return new CompetitionResult(Id?.Clone(), Owner, Participant, Points, Place);
		}

		public int CompareTo(CompetitionResult other)
		{
			int placeCompare = Place.CompareTo(other.Place);
			if (placeCompare != 0)
				return placeCompare;
			return Points.CompareTo(other.Points);
		}

		#endregion
	}
}

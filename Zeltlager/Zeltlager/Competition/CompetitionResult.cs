using System;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using DataPackets;
	using Newtonsoft.Json;
		using UAM;

	[Editable("Ergebnis", NameProperty = nameof(ParticipantName))]
	public class CompetitionResult : Editable<CompetitionResult>, IComparable<CompetitionResult>
	{
		[JsonIgnore]
		public PacketId Id { get; set; }

		[Editable("Punkte")]
		public int? Points { get; set; }

		[Editable("Platzierung")]
		public int? Place { get; set; }
		
		// todo json ref
		public Participant Participant { get; set; }

		// todo json ref
		public Rankable Owner { get; set; }

		[JsonIgnore]
		public string PointsString => Points?.ToString() ?? "-";
		[JsonIgnore]
		public string PlaceString => Place?.ToString() ?? "-";
		[JsonIgnore]
		public string ParticipantName => Participant.Name;

		public CompetitionResult() {}

		public CompetitionResult(PacketId id, Rankable owner, Participant p, int? points = null, int? place = null)
		{
			Id = id;
			Points = points;
			Place = place;
			Participant = p;
			Owner = owner;
		}

		public override CompetitionResult Clone()
		{
			return new CompetitionResult(Id?.Clone(), Owner, Participant, Points, Place);
		}

		public int CompareTo(CompetitionResult other)
		{
			if (Place.HasValue)
			{
				if (!other.Place.HasValue)
					return -1;
				return Place.CompareTo(other.Place);
			}
			if (other.Place.HasValue)
				return 1;
			if (Points.HasValue)
			{
				if (!other.Points.HasValue)
					return -1;
				return Points.CompareTo(other.Points);
			}
			if (other.Points.HasValue)
				return 1;
			return 0;
		}
	}
}

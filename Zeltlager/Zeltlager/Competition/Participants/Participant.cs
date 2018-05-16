using System;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Client;
	using DataPackets;
	using Newtonsoft.Json;
	
	/// <summary>
	/// represents a participant in a comptetion, could be a tent, a mixed group or a single person
	/// </summary>
	public abstract class Participant : Editable<Participant>, ISearchable, IEquatable<Participant>, IDeletable
	{

		[JsonIgnore]
		public PacketId Id { get; set; }

		// todo json reference
		protected Competition competition;

		public virtual string Name { get; set; }

		[JsonIgnore]
		public string SearchableText => Name;

		[JsonIgnore]
		public string SearchableDetail => "";

		public bool IsVisible { get; set; } = true;

		public Participant() {}

		public Participant(PacketId id, Competition competition)
		{
			this.competition = competition;
			Id = id;
		}

		public LagerClient GetLagerClient()
		{
			return competition.GetLagerClient();
		}

		public Competition GetCompetition()
		{
			return competition;
		}

		public static Participant GetFromString(LagerClient lager, string str)
		{
			return lager.CompetitionHandler.GetParticipantFromName(str);
		}

		public override string ToString()
		{
			return Name;
		}

		public override abstract Participant Clone();

		public abstract bool Equals(Participant other);
	}
}

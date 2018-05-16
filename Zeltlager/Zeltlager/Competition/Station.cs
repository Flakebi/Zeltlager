using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace Zeltlager.Competition
{
	using Client;
	using DataPackets;
	using Newtonsoft.Json;
		using UAM;

	/// <summary>
	/// Represents one station in the competition and the results achieved there.
	/// </summary>
	[Editable("Station")]
	public class Station : Rankable, ISearchable, IDeletable
	{
		[JsonProperty]
		Competition competition;

		[Editable("Name")]
		public string Name { get; set; }

		[Editable("Betreuer")]
		// todo json reference
		public Member Supervisor { get; set; }

		public bool IsVisible { get; set; } = true;

		[JsonIgnore]
		public IReadOnlyList<Member> SupervisorList => competition.GetLagerClient().VisibleSupervisors;

		[JsonIgnore]
		public string SearchableText => Name;

		[JsonIgnore]
		public string SearchableDetail => "Betreuer: " + Supervisor.Name;

		public Station() 
		{
			Ranking = new Ranking();
		}

		public Station(PacketId id, string name, Competition competition) :
            this(id, name, competition, new Ranking())
        { }

        public Station(PacketId id, string name, Competition competition, Member supervisor) :
            this(id, name, competition, new Ranking())
        {
            Supervisor = supervisor;
        }

        Station(PacketId id, string name, Competition competition, Ranking ranking)
		{
			Id = id;
			Name = name;
			this.competition = competition;
			Ranking = ranking;
		}

		public override void AddResult(CompetitionResult cr)
		{
			Ranking.AddResult(cr);
			Ranking.Results.Sort();
		}

		public override IReadOnlyList<Participant> GetParticipants()
		{
			return competition.GetParticipants();
		}

		public Competition GetCompetition()
		{
			return competition;
		}

		public LagerClient GetLagerClient()
		{
			return competition.GetLagerClient();
		}

		public override Rankable Clone()
		{
			return new Station(Id?.Clone(), Name, competition, Ranking);
		}

		public override string ToString()
		{
			return string.Format("Station {0}", Name);
		}
	}
}

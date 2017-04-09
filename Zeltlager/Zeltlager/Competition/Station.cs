using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Client;
	using DataPackets;
	using Serialisation;
	using UAM;

	/// <summary>
	/// represents one station in the competition and the results achieved there
	/// </summary>
	[Editable("Station")]
	public class Station : Rankable, ISearchable, IDeletable
	{
		[Serialisation(Type = SerialisationType.Reference)]
		Competition competition;

		[Editable("Name")]
		[Serialisation]
		public string Name { get; set; }

		[Editable("Betreuer")]
		[Serialisation(Type = SerialisationType.Reference)]
		public Member Supervisor { get; set; }

		[Serialisation]
		public bool IsVisible { get; set; } = true;

		public IReadOnlyList<Member> SupervisorList => competition.GetLagerClient().Supervisors;

		public string SearchableText => Name;

		public string SearchableDetail => "Betreuer: " + Supervisor.Name;

		protected static Task<Station> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.CompetitionHandler.GetStationFromPacketId(id));
		}

		public Station() 
		{
			Ranking = new Ranking();
		}

		public Station(LagerClientSerialisationContext context) : this() {}

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

		public override void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			competition.AddStation(this);
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
	}
}

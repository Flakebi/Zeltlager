using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Client;
	using UAM;
	using Serialisation;
	using DataPackets;

	[Editable("Wettkampf")]
	public class Competition : Rankable, ISearchable, IDeletable
	{
		LagerClient lager;

		[Editable("Name")]
		[Serialisation]
		public string Name { get; set; }

		public List<Participant> Participants { get; set; }

		public List<Station> Stations { get; set; }

		protected static Task<Competition> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.CompetitionHandler.GetCompetitionFromPacketId(id));
		}

		public Competition() 
		{
			Ranking = new Ranking();
			Participants = new List<Participant>();
			Stations = new List<Station>();
		}

		public Competition(LagerClientSerialisationContext context) : this() {}

		public Competition(PacketId id, string name, LagerClient lager)
		{
			Id = id;
			this.lager = lager;
			Name = name;
			Participants = new List<Participant>();
			Stations = new List<Station>();
			Ranking = new Ranking();
		}

		public Competition(LagerClient lager, PacketId id, string name, List<Participant> participants, List<Station> stations, Ranking ranking)
		{
			this.lager = lager;
			Id = id;
			Name = name;
			Participants = participants;
			Stations = stations;
			Ranking = ranking;
		}

		public override void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			lager = context.LagerClient;
			context.LagerClient.CompetitionHandler.AddCompetition(this);
		}

		public override void AddResult(CompetitionResult cr)
		{
			Ranking.AddResult(cr);
		}

		public void AddStation(Station station) => Stations.Add(station);

		public void RemoveStation(Station station) => Stations.Remove(station);

		public void AddParticipant(Participant participant) => Participants.Add(participant);

		public void RemoveParticipant(Participant participant) => Participants.Remove(participant);

		public override IReadOnlyList<Participant> GetParticipants()
		{
			return Participants;
		}

		public LagerClient GetLagerClient()
		{
			return lager;
		}

		#region Interface implementation

		public override Rankable Clone()
		{
			return new Competition(lager, Id, Name, Participants, Stations, Ranking);
		}

		public string SearchableText { get { return Name; } }

		public string SearchableDetail { get { return ""; } }

		[Serialisation]
		public bool IsVisible { get; set; } = true;

		#endregion
	}
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Client;
	using UAM;
	using Serialisation;
	using DataPackets;

	[Editable("Wettkampf")]
	public class Competition : Rankable, IEditable<Competition>, ISearchable
	{
		public LagerClient Lager { get; private set; }

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Editable("Name")]
		[Serialisation]
		public string Name { get; set; }

		// [Editable("Teilnehmer")]
		// [Serialisation]
		public List<Participant> Participants { get; set; }

		// [Editable("Stationen")]
		[Serialisation(Type = SerialisationType.Reference)]
		public List<Station> Stations { get; set; }

		public Ranking Ranking { get; set; }

		protected static Task<Competition> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.CompetitionHandler.GetCompetitionFromPacketId(id));
		}

		public Competition() 
		{
			Ranking = new Ranking();
		}

		public Competition(LagerClientSerialisationContext context) : this() {}

		public Competition(PacketId id, string name, LagerClient lager)
		{
			Id = id;
			this.Lager = lager;
			Name = name;
			Participants = new List<Participant>();
			Stations = new List<Station>();
			Ranking = new Ranking();
		}

		public Competition(LagerClient lager, PacketId id, string name, List<Participant> participants, List<Station> stations, Ranking ranking)
		{
			Lager = lager;
			Id = id;
			Name = name;
			Participants = participants;
			Stations = stations;
			Ranking = ranking;
		}

		public override void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			Lager = context.LagerClient;
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

		#region Interface implementation

		public async Task OnSaveEditing(
            Serialiser<LagerClientSerialisationContext> serialiser,
            LagerClientSerialisationContext context, Competition oldObject)
		{
			DataPacket packet;
			if (oldObject != null)
				packet = await EditPacket.Create(serialiser, context, this);
			else
				packet = await AddPacket.Create(serialiser, context, this);
			await context.LagerClient.AddPacket(packet);
		}

		public Competition Clone()
		{
			return new Competition(Lager, Id, Name, Participants, Stations, Ranking);
		}

		public string SearchableText { get { return Name; } }

		public string SearchableDetail { get { return ""; } }

		#endregion
	}
}

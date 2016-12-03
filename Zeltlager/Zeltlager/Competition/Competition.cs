using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Client;
	using UAM;
	using Serialisation;
	using Zeltlager.DataPackets;

	[Editable("Wettkampf")]
	public class Competition : IEditable<Competition>, ISearchable
	{
		private LagerClient lager;

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Editable("Name")]
		[Serialisation]
		public string Name { get; set; }

		// TODO [Editable("Teilnehmer")]
		[Serialisation]
		public List<Participant> Participants { get; set; }
		[Editable("Stationen")]
		[Serialisation(Type = SerialisationType.Reference)]
		public List<Station> Stations { get; set; }
		[Serialisation]
		public Ranking Ranking { get; set; }

		protected static Task<Competition> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.CompetitionHandler.GetCompetitionFromPacketId(id));
		}

		public Competition() {}

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

		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			lager = context.LagerClient;
			context.LagerClient.CompetitionHandler.AddCompetition(this);
		}

		public void AddStation(Station station) => Stations.Add(station);

		public void RemoveStation(Station station) => Stations.Remove(station);

		public void AddParticipant(Participant participant) => Participants.Add(participant);

		public void RemoveParticipant(Participant participant) => Participants.Remove(participant);

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
			return new Competition(Id, Name, lager);
		}

		public string SearchableText { get { return Name; } }

		public string SearchableDetail { get { return ""; } }

		#endregion
	}
}

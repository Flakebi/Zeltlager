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
		public string Name;

		// TODO [Editable("Teilnehmer")]
		public List<Participant> Participants;
		[Editable("Stationen")]
		public List<Station> Stations;
		public Ranking Ranking;

		public Competition() {}

		public Competition(LagerClientSerialisationContext context) : this() {}

		public Competition(PacketId id, string name, LagerClient lager)
		{
			Id = id;
			this.lager = lager;
			Name = name;
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

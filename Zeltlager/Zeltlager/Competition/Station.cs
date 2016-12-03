using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Serialisation;
	using UAM;
	using Zeltlager.DataPackets;

	/// <summary>
	/// represents one station in the competition and the results achieved there
	/// </summary>
	[Editable("Station")]
	public class Station : ISearchable, IEditable<Station>
	{
		[Serialisation(Type = SerialisationType.Reference)]
		Competition competition;

		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set;}

		[Editable("Name")]
		[Serialisation]
		public string Name { get; set; }

		[Editable("Betreuer")]
		[Serialisation(Type = SerialisationType.Reference)]
		public Member Supervisor { get; set; }

		[Serialisation]
		public Ranking Ranking { get; set; }

		protected static Task<Station> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.CompetitionHandler.GetStationFromPacketId(id));
		}

		public Station() {}

		public Station(LagerClientSerialisationContext context) : this() {}

		public Station(PacketId id, string name, Competition competition)
		{
			Id = id;
			Name = name;
			this.competition = competition;
			Ranking =  new Ranking();
		}

		Station(PacketId id, string name, Competition competition, Ranking ranking)
		{
			Id = id;
			Name = name;
			this.competition = competition;
			Ranking = ranking;
		}

		public void Add(LagerClientSerialisationContext context)
		{
			Id = context.PacketId;
			competition.AddStation(this);
		}

		public void AddResult(Participant participant, int? points = null, int? place = null)
		{
			
		}

		#region Interface implementation

		public string SearchableText => Name;

		public string SearchableDetail => "";

		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, Station oldObject)
		{
			DataPacket packet;
			if (oldObject != null)
				packet = await EditPacket.Create(serialiser, context, this);
			else
				packet = await AddPacket.Create(serialiser, context, this);
			await context.LagerClient.AddPacket(packet);
		}

		public Station Clone()
		{
			return new Station(Id, Name, competition, Ranking);
		}

		#endregion
	}
}

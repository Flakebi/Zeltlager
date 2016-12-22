using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using Serialisation;
	using UAM;
	using Zeltlager.Client;
	using Zeltlager.DataPackets;

	/// <summary>
	/// represents a participant in a comptetion, could be a tent, a mixed group or a single person
	/// </summary>
	public abstract class Participant : ISearchable, IEditable<Participant>
	{
		// TODO Participants serialisieren
		[Serialisation(Type = SerialisationType.Id)]
		public PacketId Id { get; set; }

		[Serialisation(Type = SerialisationType.Reference)]
		protected Competition competition;

		public virtual string Name { get; set; }

		public Participant() {}

		public Participant(LagerClientSerialisationContext context) : this() {}

		public Participant(PacketId id, Competition competition)
		{
			this.competition = competition;
			Id = id;
		}

		public void Add(LagerClientSerialisationContext context) 
		{
			Id = context.PacketId;
			competition.AddParticipant(this);
		}

		static Task<Participant> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult(context.LagerClient.CompetitionHandler.GetParticipantFromId(id));
		}

		public LagerClient GetLagerClient()
		{
			return competition.GetLagerClient();
		}

		public Competition GetCompetition()
		{
			return competition;
		}

		#region Interface implementation

		public string SearchableText => Name;

		public string SearchableDetail => "";

		public async Task OnSaveEditing(
			Serialiser<LagerClientSerialisationContext> serialiser,
			LagerClientSerialisationContext context, Participant oldObject)
		{
			DataPacket packet;
			if (oldObject != null)
				packet = await EditPacket.Create(serialiser, context, this);
			else
				packet = await AddPacket.Create(serialiser, context, this);
			await context.LagerClient.AddPacket(packet);
		}

		public abstract Participant Clone();

		#endregion
	}
}
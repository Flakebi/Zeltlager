using System.Threading.Tasks;
using System;

namespace Zeltlager.Competition
{
	using Serialisation;
	using UAM;
	using Zeltlager.Client;
	using Zeltlager.DataPackets;

	/// <summary>
	/// represents a participant in a comptetion, could be a tent, a mixed group or a single person
	/// </summary>
	public abstract class Participant : Editable<Participant>, ISearchable, IEquatable<Participant>, IDeletable
	{
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

		public static Participant GetFromString(LagerClient lager, string str)
		{
			return lager.CompetitionHandler.GetParticipantFromName(str);
		}

		public override string ToString()
		{
			return Name;
		}

		#region Interface implementation

		public string SearchableText => Name;

		public string SearchableDetail => "";

		[Serialisation]
		public bool IsVisible { get; set; } = true;

		public override abstract Participant Clone();

		public abstract bool Equals(Participant other);

		#endregion
	}
}

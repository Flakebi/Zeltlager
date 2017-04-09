using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using DataPackets;
	using Serialisation;

	/// <summary>
	/// a participant in a competition that respresents a mixed group with an individual name
	/// </summary>
	public class GroupParticipant : Participant
	{
		public override string Name 
		{ 
			get { return name; } 
			set { name = value; } 
		}

		[Serialisation]
		string name;

		static Task<GroupParticipant> GetFromId(LagerClientSerialisationContext context, PacketId id)
		{
			return Task.FromResult((GroupParticipant)context.LagerClient.CompetitionHandler.GetParticipantFromId(id));
		}

		public GroupParticipant() {}

		public GroupParticipant(LagerClientSerialisationContext context) : base(context) {}

		public GroupParticipant(PacketId id, string name, Competition competition) : base(id, competition)
		{
			this.name = name;
		}

		public override Participant Clone()
		{
			return new GroupParticipant(Id?.Clone(), name, competition);
		}

		public override bool Equals(Participant other)
		{
			if (other is GroupParticipant)
			{
				return name.Equals(((GroupParticipant)other).name);
			}
			return false;
		}
	}
}

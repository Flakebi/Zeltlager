using System;
using Zeltlager.Serialisation;
namespace Zeltlager.Competition
{
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

		public GroupParticipant() {}

		public GroupParticipant(LagerClientSerialisationContext context) : base(context) {}

		public GroupParticipant(DataPackets.PacketId id, string name, Competition competition) : base(id, competition)
		{
			this.name = name;
		}

		public override Participant Clone()
		{
			return new GroupParticipant(Id, name, competition);
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

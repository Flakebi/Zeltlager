using System.Threading.Tasks;

namespace Zeltlager.Competition
{
	using DataPackets;
	using Newtonsoft.Json;
	
	public class TentParticipant : Participant
	{
		[JsonIgnore]
		public override string Name => Tent.Display;

		// todo json reference
		public Tent Tent { get; set; }

		public TentParticipant() { }

		public TentParticipant(PacketId id, Tent tent, Competition competition) : base(id, competition)
		{
			Tent = tent;
		}

		public override Participant Clone()
		{
			return new TentParticipant(Id?.Clone(), Tent, competition);
		}

		public override bool Equals(Participant other)
		{
			if (other is TentParticipant)
			{
				return Tent.Equals(((TentParticipant)other).Tent);
			}
			return false;
		}
	}
}

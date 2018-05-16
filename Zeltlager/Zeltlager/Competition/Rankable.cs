using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zeltlager
{
	using Competition;
	using DataPackets;
	
	public abstract class Rankable : Editable<Rankable>
	{
		public PacketId Id { get; set; }

		public virtual Ranking Ranking { get; set; }

		public abstract void AddResult(CompetitionResult cr);

		public abstract IReadOnlyList<Participant> GetParticipants();
	}
}

using System.Collections.Generic;
using System.Linq;

namespace Zeltlager.Competition
{
	using Client;
	using DataPackets;

	/// <summary>
	/// collects all the competitions of one lager
	/// </summary>
	public class CompetitionHandler
	{
		LagerClient lager;
		public List<Competition> Competitions = new List<Competition>();

		public CompetitionHandler(LagerClient lager)
		{
			this.lager = lager;
		}

		public void AddCompetition(Competition comp)
		{
			Competitions.Add(comp);
		}

		public void AddCompetitionResult(CompetitionResult cr)
		{
			cr.Owner.AddResult(cr);
		}

		public Competition GetCompetitionFromPacketId(PacketId id)
		{
			return Competitions.Find(x => x.Id == id);
		}

		public Station GetStationFromPacketId(PacketId id)
		{
			return Competitions.SelectMany(c => c.Stations).FirstOrDefault(x => x.Id == id);
		}

		public Rankable GetRankableFromId(PacketId id)
		{
			return (Rankable)GetStationFromPacketId(id) ?? GetCompetitionFromPacketId(id);
		}

		public Participant GetParticipantFromId(PacketId id)
		{
			return Competitions.SelectMany(c => c.Participants).First(x => x.Id == id);
		}

		public Participant GetParticipantFromName(string name)
		{
			return Competitions.SelectMany(c => c.Participants).First(x => x.Name == name);
		}
	}
}

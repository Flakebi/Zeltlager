using System.Collections.Generic;
using System.Linq;

namespace Zeltlager.Competition
{
	using Client;
	using Zeltlager.DataPackets;

	/// <summary>
	/// collects all the competitions of one lager
	/// </summary>
	public class CompetitionHandler
	{
		LagerClient lager;
		public List<Competition> Competitions;

		public CompetitionHandler(LagerClient lager)
		{
			this.lager = lager;
			this.Competitions = new List<Competition>();
		}

		public void AddCompetition(Competition comp)
		{
			Competitions.Add(comp);
		}

		public void RemoveCompetition(Competition comp)
		{
			// Competitions.Remove(comp);
		}

		public void AddCompetitionResult(CompetitionResult cr)
		{
			Competition comp = cr.Owner as Competition;
			if (comp != null)
			{
				comp.AddResult(cr);
			} 
			else 
			{
				Station stat = (Station) cr.Owner;
				stat.AddResult(cr);
			}
		}

		public Competition GetCompetitionFromPacketId(PacketId id)
		{
			return Competitions.Find(x => x.Id == id);
		}

		public Station GetStationFromPacketId(PacketId id)
		{
			return Competitions.SelectMany(c => c.Stations).First(x => x.Id == id);
		}

		public Rankable GetRankableFromId(PacketId id)
		{
			return (Rankable)GetStationFromPacketId(id) ?? GetCompetitionFromPacketId(id);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Zeltlager.Serialisation;

namespace Zeltlager.Competition
{
	public class Ranking
	{
		[Serialisation]
		public List<CompetitionResult> Results { get; set; }

		public Ranking() 
		{
			Results = new List<CompetitionResult>();
		}

		public void AddResult(CompetitionResult cr) 
		{
			Results.Add(cr);
		}

		public void RemoveResult(Participant p)
		{
			// TODO wie tun wir das?
		}

		public void Rank(bool increasing)
		{
			IEnumerable<CompetitionResult> x;
			if (!increasing)
				x = Results.OrderByDescending(r => r.Points);
			else
				x = Results.OrderBy(r => r.Points);
			x.Select((cr, i) => cr.Place = i);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Zeltlager.Serialisation;

namespace Zeltlager.Competition
{
	public class Ranking
	{
		[Serialisation]
		public List<CompetitionResult> Results { get; private set; }

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
	}
}

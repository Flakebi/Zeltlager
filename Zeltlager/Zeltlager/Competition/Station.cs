using System.Collections.Generic;

namespace Zeltlager.Competition
{
	/// <summary>
	/// represents one station in the competition and the results achieved there
	/// </summary>
	public class Station
	{
		string name;
		List<CompetitionResult> results;

		public Station(string name)
		{
			this.name = name;
			results = new List<CompetitionResult>();
		}

		public void AddResult(Participant par, int result)
		{
			results.Add(new CompetitionResult(par, result));
		}
	}
}

using System.Collections.Generic;

namespace Zeltlager.Competition
{
	using Client;

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
		}

		public void AddCompetition(Competition comp)
		{
			Competitions.Add(comp);
		}

		public void RemoveCompetition(Competition comp)
		{
			Competitions.Remove(comp);
			// TODO: wie testen ob das geht?
		}
	}
}

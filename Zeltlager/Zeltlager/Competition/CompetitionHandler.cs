namespace Zeltlager.Competition
{
	using System.Collections.Generic;
	using Client;

	/// <summary>
	/// collects all the competitions of one lager
	/// </summary>
	public class CompetitionHandler : ILagerPart
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

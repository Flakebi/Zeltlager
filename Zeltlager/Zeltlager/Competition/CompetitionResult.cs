using System;
using Zeltlager.UAM;
namespace Zeltlager.Competition
{
	public class CompetitionResult
	{
		int points;
		int? place;

		public CompetitionResult(int points, int? place)
		{
			this.points = points;
			this.place = place;
		}
	}
}

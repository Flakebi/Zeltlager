using System;
using Zeltlager.UAM;
namespace Zeltlager.Competition
{
	/// <summary>
	/// saves the result of a participant for a certain competition
	/// </summary>
	public class CompetitionResult
	{
		Participant participant;
		int result;

		public CompetitionResult(Participant par, int res)
		{
			participant = par;
			result = res;
		}
	}
}

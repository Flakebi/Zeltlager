using System;
namespace Zeltlager.Competition
{
	/// <summary>
	/// represents a participant in a comptetion, could be a tent, a mixed group or a single person
	/// </summary>
	public class Participant
	{
		string name;

		public Participant(string name)
		{
			this.name = name;
		}
	}
}

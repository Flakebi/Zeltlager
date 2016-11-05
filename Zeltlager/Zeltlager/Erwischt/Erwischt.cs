using System.Collections.Generic;

namespace Zeltlager.Erwischt
{
	using Client;

	/// <summary>
	/// Data associated to each member for the Erwischt game.
	/// </summary>
	class MemberData
	{
		/// <summary>
		/// If this member is still alive.
		/// </summary>
		public bool Alive;
		/// <summary>
		/// The next target of this member.
		/// </summary>
		public Member Target;
	}

	public class Erwischt
	{
		LagerClient lager;
		Dictionary<Member, MemberData> memberData;

		public Erwischt(LagerClient lager)
		{
			this.lager = lager;
			memberData = new Dictionary<Member, MemberData>();
		}
	}
}

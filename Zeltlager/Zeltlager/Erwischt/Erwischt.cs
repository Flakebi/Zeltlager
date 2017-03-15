using System.Collections.Generic;

namespace Zeltlager.Erwischt
{
	using Client;

	/// <summary>
	/// One instance of an Erwischt game.
	/// </summary>
	public class Erwischt
	{
		public string Name { get; set; }
		public List<ErwischtMember> Participants { get; set; }

		//TODO make editable, lagerclient?, pass Members to constructor & initialize list

		public Erwischt(string name)
		{
			Name = name;
			Participants = new List<ErwischtMember>();
		}
	}
}

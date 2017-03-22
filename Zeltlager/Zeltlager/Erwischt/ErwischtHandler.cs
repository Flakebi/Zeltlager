using System;
using System.Collections.Generic;
using Zeltlager.Client;
using System.Linq;
namespace Zeltlager.Erwischt
{
	/// <summary>
	/// Handles all different Erwischt games.
	/// </summary>
	public class ErwischtHandler
	{
		LagerClient lager;
		public List<Erwischt> Games { get; private set; }
		public List<Erwischt> VisibleGames => Games.Where(g => g.IsVisible).ToList();
		public Erwischt CurrentGame { get; set; }

		public ErwischtHandler(LagerClient lager)
		{
			this.lager = lager;
			Games = new List<Erwischt>();
			CurrentGame = null;
		}
	}
}

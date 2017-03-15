using System;
using System.Collections.Generic;
using Zeltlager.Client;
namespace Zeltlager.Erwischt
{
	/// <summary>
	/// Handles all different Erwischt games.
	/// </summary>
	public class ErwischtHandler
	{
		LagerClient lager;
		public List<Erwischt> Games { get; private set; }
		int currentGameIndex;

		public ErwischtHandler(LagerClient lager)
		{
			this.lager = lager;
			Games = new List<Erwischt>();
			currentGameIndex = -1;
		}

		public Erwischt GetCurrentGame()
		{
			// TODO
			return null;
		}
	}
}
